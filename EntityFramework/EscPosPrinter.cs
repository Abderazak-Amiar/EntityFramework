namespace EntityFramework
{
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;
    using SkiaSharp;
    using Svg.Skia;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public static class EscPosPrinter
    {
        public static void PrintTicketEscPos(string? printerName, User user, int dotsPerLine = 576, bool cut = false)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            QuestPdfPrinter.GeneratePdfAndOpen(user);
        }
    }

    public static class QuestPdfPrinter
    {
        public static void GeneratePdfAndOpen(User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            try
            {
                var document = BuildDocument(user);
                var pdfBytes = GeneratePdfBytes(document);
                OpenPdfInDefaultViewer(pdfBytes, $"user_ticket_{user.Id}.pdf");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed generating PDF: " + ex.Message, ex);
            }
        }

        public static byte[] CreatePdfBytes(User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            try
            {
                var document = BuildDocument(user);
                return GeneratePdfBytes(document);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed creating PDF bytes: " + ex.Message, ex);
            }
        }

        private static IDocument BuildDocument(User user)
        {
            // Culture for French formatting
            var fr = CultureInfo.GetCultureInfo("fr-FR");

            // Helper: format decimals smartly: if fractional part is zero show integer (no decimals),
            // otherwise show one decimal using the French culture.
            static string FormatDecimalSmart(decimal value, CultureInfo ci)
            {
                return decimal.Truncate(value) == value
                    ? value.ToString("N0", ci)   // no decimals
                    : value.ToString("N1", ci);  // one decimal
            }

            // Precompute safe strings so we never call .Value on a nullable without checking.
            string? NameText = string.IsNullOrWhiteSpace(user.Name) ? null : user.Name;
            string? PhoneText = string.IsNullOrWhiteSpace(user.Phone) ? null : user.Phone;
            string? AddressText = string.IsNullOrWhiteSpace(user.Address) ? null : user.Address;

            string? NbrBagsText = user.NbrBags != 0m ? FormatDecimalSmart(user.NbrBags, fr) : null;
            string? NbrContainersText = string.IsNullOrWhiteSpace(user.NbrContainers) ? null : user.NbrContainers;
            string? WeightText = (user.Weight.HasValue && user.Weight.Value != 0m) ? FormatDecimalSmart(user.Weight.Value, fr) : null;
            string? NbrLitersText = (user.NbrLiters.HasValue && user.NbrLiters.Value != 0) ? user.NbrLiters.Value.ToString(fr) : null;
            string? UnitPriceText = (user.UnitPriceLiter.HasValue && user.UnitPriceLiter.Value != 0m) ? FormatDecimalSmart(user.UnitPriceLiter.Value, fr) : null;
            string? PayedLitersText = (user.PayedLiters.HasValue && user.PayedLiters.Value != 0) ? user.PayedLiters.Value.ToString(fr) : null;
            string? AmountDueText = (user.AmountDue.HasValue && user.AmountDue.Value != 0m) ? FormatDecimalSmart(user.AmountDue.Value, fr) : null;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(new PageSize(width: 204, height: 2024));
                    page.Margin(6);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                    // Header
                    page.Header().Column(col =>
                    {
                        byte[]? imageBytes = null;
                        var baseDir = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory;
                        var candidates = new[]
                        {
                            Path.Combine(baseDir, "Images", "logoM3insra.svg"),
                            Path.Combine(baseDir, "logoM3insra.svg")
                        };

                        foreach (var path in candidates)
                        {
                            try
                            {
                                if (!File.Exists(path)) continue;
                                var raw = File.ReadAllBytes(path);
                                imageBytes = path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                                    ? RenderSvgToPng(raw, targetWidthPx: 800)
                                    : raw;
                                Debug.WriteLine($"QuestPDF: using image file {path}");
                                break;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"QuestPDF: failed reading {path}: {ex.Message}");
                            }
                        }

                        if (imageBytes == null)
                        {
                            try
                            {
                                var asm = Assembly.GetExecutingAssembly();
                                var names = asm.GetManifestResourceNames();
                                var found = names.FirstOrDefault(n => n.EndsWith("logoM3insra.svg", StringComparison.OrdinalIgnoreCase));
                                if (found != null)
                                {
                                    using var s = asm.GetManifestResourceStream(found);
                                    if (s != null)
                                    {
                                        using var ms = new MemoryStream();
                                        s.CopyTo(ms);
                                        var raw = ms.ToArray();
                                        imageBytes = found.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                                            ? RenderSvgToPng(raw, targetWidthPx: 800)
                                            : raw;
                                        Debug.WriteLine($"QuestPDF: using embedded resource {found}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"QuestPDF: embedded resource load failed: {ex.Message}");
                            }
                        }

                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            col.Item().AlignCenter().Width(70).Image(imageBytes);
                        }
                        else
                        {
                            col.Item().PaddingTop(0).Text("Logo de l'entreprise").FontSize(12).AlignCenter();
                        }

                        col.Item().Text("Ticket").FontSize(14).Bold().AlignCenter();
                        col.Item().PaddingVertical(4).LineHorizontal(1);
                    });

                    page.Content().Column(col =>
                    {
                        void AddIfNotEmpty(string? text)
                        {
                            if (!string.IsNullOrWhiteSpace(text))
                                col.Item().Text(text);
                        }

                        col.Item().PaddingVertical(4).LineHorizontal(1);

                        // Always include ID
                        col.Item().Text($"N°: {user.Id}");

                        // Use precomputed strings only (never access nullable.Value here)
                        AddIfNotEmpty(NameText is null ? null : $"Nom: {NameText}");
                        AddIfNotEmpty(PhoneText is null ? null : $"Téléphone: {PhoneText}");
                        AddIfNotEmpty(AddressText is null ? null : $"Adresse: {AddressText}");
                        AddIfNotEmpty(NbrBagsText is null ? null : $"Sacs: {NbrBagsText}");
                        AddIfNotEmpty(NbrContainersText is null ? null : $"Bidons: {NbrContainersText}");
                        AddIfNotEmpty(WeightText is null ? null : $"Poids: {WeightText}");
                        AddIfNotEmpty(NbrLitersText is null ? null : $"Litres: {NbrLitersText}");
                        AddIfNotEmpty(UnitPriceText is null ? null : $"Prix/L: {UnitPriceText}");
                        AddIfNotEmpty(PayedLitersText is null ? null : $"Litres payés: {PayedLitersText}");
                        AddIfNotEmpty(AmountDueText is null ? null : $"Montant dû: {AmountDueText}");

                        col.Item().PaddingTop(6).Text($"Imprimé le: {DateTime.Now.ToString("f", fr)}").FontSize(9);
                        col.Item().PaddingVertical(6).LineHorizontal(1);
                        col.Item().Text("Merci").AlignCenter().FontSize(9);
                    });
                });
            });
        }

        private static byte[] RenderSvgToPng(byte[] svgBytes, int targetWidthPx = 0)
        {
            if (svgBytes == null || svgBytes.Length == 0)
                throw new ArgumentNullException(nameof(svgBytes));

            using var ms = new MemoryStream(svgBytes);
            var svg = new SKSvg();
            svg.Load(ms);

            var picture = svg.Picture;
            if (picture == null)
                throw new InvalidOperationException("SVG could not be parsed by SKSvg.");

            var rect = picture.CullRect;
            float origWidth = rect.Width;
            float origHeight = rect.Height;
            if (origWidth <= 0 || origHeight <= 0)
                throw new InvalidOperationException("SVG has invalid dimensions.");

            var scale = 1f;
            if (targetWidthPx > 0)
                scale = targetWidthPx / origWidth;

            int outWidth = Math.Max(1, (int)Math.Ceiling(origWidth * scale));
            int outHeight = Math.Max(1, (int)Math.Ceiling(origHeight * scale));

            var info = new SKImageInfo(outWidth, outHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var bitmap = new SKBitmap(info);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);
            if (scale != 1f)
                canvas.Scale(scale);
            canvas.DrawPicture(picture);
            canvas.Flush();

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static byte[] GeneratePdfBytes(IDocument document)
        {
            using var ms = new MemoryStream();
            document.GeneratePdf(ms);
            return ms.ToArray();
        }

        private static void OpenPdfInDefaultViewer(byte[] pdfBytes, string fileName = "ticket.pdf")
        {
            if (pdfBytes is null) throw new ArgumentNullException(nameof(pdfBytes));
            var tmp = Path.Combine(Path.GetTempPath(), fileName);
            File.WriteAllBytes(tmp, pdfBytes);
            var psi = new ProcessStartInfo(tmp) { UseShellExecute = true };
            Process.Start(psi);
        }
    }
}
