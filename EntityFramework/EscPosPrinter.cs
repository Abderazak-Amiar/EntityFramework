namespace EntityFramework
{
    using Microsoft.EntityFrameworkCore;
    using QuestPDF.Fluent;
    using QuestPDF.Helpers;
    using QuestPDF.Infrastructure;
    using SkiaSharp;
    using Svg.Skia;
    using System;
    using System.Collections.Generic;
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

        // Print a Vente receipt using QuestPdfPrinter helper for ventes

        public static void PrintVenteReceipt(string? printerName, Vente vente, int dotsPerLine = 576, bool cut = false)
        {
            if (vente is null) throw new ArgumentNullException(nameof(vente));
            QuestPdfPrinter.GeneratePdfAndOpenVente(vente);
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

        // New: generate & open a small PDF receipt for a Vente using same header logic as user receipts

        public static void GeneratePdfAndOpenVente(Vente vente) 
        {
            if (vente is null) throw new ArgumentNullException(nameof(vente));

            try
            {
                var fr = CultureInfo.GetCultureInfo("fr-FR");

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(new PageSize(width: 204, height: 400));
                        page.Margin(6);
                        page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                        // Header: reuse logo/company layout similar to user receipt
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

                            // Company info from Parameters (safe read)
                            try
                            {
                                using var ctx = new DataContext();
                                var parameters = ctx.Parameters?.FirstOrDefault(p => p.Id == 1);
                                if (parameters != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(parameters.CompanyName))
                                        col.Item().Text(parameters.CompanyName).FontSize(11).Bold().AlignCenter();

                                    if (!string.IsNullOrWhiteSpace(parameters.CompanyAddress))
                                        col.Item().Text(parameters.CompanyAddress).FontSize(9).AlignCenter();

                                    if (!string.IsNullOrWhiteSpace(parameters.CompanyPhone))
                                        col.Item().Text(parameters.CompanyPhone).FontSize(9).AlignCenter();
                                }
                            }
                            catch
                            {
                                // swallow
                            }

                            col.Item().PaddingTop(8).Text("Reçu de vente").FontSize(12).Bold().AlignCenter();
                            col.Item().PaddingVertical(4).LineHorizontal(1);
                        });

                        page.Content().Column(col =>
                        {
                            col.Item().PaddingVertical(4).LineHorizontal(1);

                            col.Item().Column(c =>
                            {
                                c.Item().Text($"Date : {vente.CreatedAt.ToString("g", fr)}").FontSize(9);
                                c.Item().Text($"Litres : {vente.NbrLitres.ToString("N0", fr)}").FontSize(10);
                                c.Item().Text($"Prix/L : {vente.Prix.ToString("N2", fr)}").FontSize(10);
                                c.Item().PaddingTop(6).Text($"Montant : {vente.Montant.ToString("N2", fr)}").FontSize(11).Bold();
                            });

                            col.Item().PaddingTop(8).LineHorizontal(1);
                            col.Item().Text("Merci pour votre confiance").AlignCenter().FontSize(9).Bold();
                        });

                        // No dotted footer line — removed as requested
                    });
                });

                var pdfBytes = GeneratePdfBytes(document);
                OpenPdfInDefaultViewer(pdfBytes, $"vente_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed generating vente PDF: " + ex.Message, ex);
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

            // Calculate Rendement (olive oil yield per quintal) when both poids and litres available:
            // Rendement = (Litres * 100) / Poids  -> litres per 100kg
            string? RendementText = null;
            try
            {
                if (user.Weight.HasValue && user.Weight.Value != 0m && user.NbrLiters.HasValue && user.NbrLiters.Value != 0)
                {
                    var litres = (decimal)user.NbrLiters.Value;
                    var poids = user.Weight.Value;
                    var rendement = (litres * 100m) / poids;
                    RendementText = FormatDecimalSmart(rendement, fr);
                }
            }
            catch
            {
                RendementText = null;
            }

            // Default portion fraction read from Parameters (0..1). If missing, remains 0.
            decimal defaultPortionFraction = 0m;
            try
            {
                using var ctx = new DataContext();
                var conn = ctx.Database.GetDbConnection();
                conn.Open();

                using (var pragmaCmd = conn.CreateCommand())
                {
                    pragmaCmd.CommandText = "PRAGMA table_info('Parameters');";
                    using var r = pragmaCmd.ExecuteReader();
                    var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    while (r.Read())
                        cols.Add(r.GetString(r.GetOrdinal("name")));

                    var selectCols = new List<string>();
                    if (cols.Contains("CompanyName")) selectCols.Add("CompanyName");
                    if (cols.Contains("CompanyAddress")) selectCols.Add("CompanyAddress");
                    if (cols.Contains("CompanyPhone")) selectCols.Add("CompanyPhone");
                    if (cols.Contains("DefaultPortion")) selectCols.Add("DefaultPortion");

                    if (selectCols.Count > 0)
                    {
                        using var selectCmd = conn.CreateCommand();
                        selectCmd.CommandText = "SELECT " + string.Join(", ", selectCols) + " FROM Parameters WHERE Id = 1 LIMIT 1;";
                        using var r2 = selectCmd.ExecuteReader();
                        if (r2.Read())
                        {
                            if (selectCols.Contains("DefaultPortion") && !r2.IsDBNull(r2.GetOrdinal("DefaultPortion")))
                            {
                                try
                                {
                                    // SQLite may return double; convert safely to decimal.
                                    var val = r2.GetValue(r2.GetOrdinal("DefaultPortion"));
                                    defaultPortionFraction = Convert.ToDecimal(val);
                                }
                                catch
                                {
                                    defaultPortionFraction = 0m;
                                }
                            }
                        }
                    }
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("QuestPDF: failed reading Parameters safely: " + ex.Message);
            }

            // If DefaultPortion is stored as fraction 0..1, compute portion liters and delivered liters:
            string? PortionLitersText = null;
            string? DeliveredLitersText = null;
            if (user.NbrLiters.HasValue && user.NbrLiters.Value != 0)
            {
                var totalLiters = (decimal)user.NbrLiters.Value;

                if (defaultPortionFraction > 0m)
                {
                    var portionLiters = defaultPortionFraction * totalLiters;
                    // Delivered = produced - portion
                    var deliveredLiters = totalLiters - portionLiters;
                    if (deliveredLiters < 0m) deliveredLiters = 0m;

                    PortionLitersText = FormatDecimalSmart(portionLiters, fr);
                    DeliveredLitersText = FormatDecimalSmart(deliveredLiters, fr);
                }
            }

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

                        // Company info (from Parameters)
                        try
                        {
                            using var ctx = new DataContext();
                            var parameters = ctx.Parameters?.FirstOrDefault(p => p.Id == 1);
                            if (parameters != null)
                            {
                                if (!string.IsNullOrWhiteSpace(parameters.CompanyName))
                                    col.Item().Text(parameters.CompanyName).FontSize(11).Bold().AlignCenter();

                                if (!string.IsNullOrWhiteSpace(parameters.CompanyAddress))
                                    col.Item().Text(parameters.CompanyAddress).FontSize(9).AlignCenter();

                                if (!string.IsNullOrWhiteSpace(parameters.CompanyPhone))
                                    col.Item().Text(parameters.CompanyPhone).FontSize(9).AlignCenter();
                            }
                        }
                        catch
                        {
                            // swallow - we already logged earlier
                        }
                      
                        // Add a top margin before the "Ticket" title to increase spacing from the header
                    
                        col.Item().PaddingVertical(4).LineHorizontal(1);
                        col.Item().Text("Ticket").FontSize(14).Bold().AlignCenter();
                        col.Item().Text(user.Id.ToString(fr)).FontSize(42).Bold().AlignCenter();
                        col.Item().PaddingVertical(4).LineHorizontal(1);
                    });

                    page.Content().Column(col =>
                    {

                        // Replace the simple vertical list with a two-column label/value table.
                        col.Item().Element(containerTable =>
                        {
                            containerTable.Table(table =>
                            {
                                // Define two columns: label (fixed) and value (fill)
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(60);    // label column (px)
                                    columns.RelativeColumn();      // value column (fills)
                                });

                                // Helper to add a labeled row if the value is not empty.
                                void AddRow(string label, string? value)
                                {
                                    if (string.IsNullOrWhiteSpace(value)) return;

                                    if(label != "N°")
                                    {
                                  
                                        table.Cell().PaddingVertical(2).Text(label).FontSize(9).SemiBold();
                                        table.Cell().PaddingVertical(2).Text(value).FontSize(9);
                                    }
                                }

                                // Always include ID as first row

                                AddRow("Nom", NameText);
                                AddRow("Tél", PhoneText);
                                AddRow("Adresse", AddressText);
                                AddRow("Sacs", NbrBagsText);
                                AddRow("Bidons", NbrContainersText);
                                AddRow("Poids", WeightText);

                                // NEW: include Rendement (litres per 100kg) when calculated
                                AddRow("Rendement", !string.IsNullOrWhiteSpace(RendementText) ? $"{RendementText} L/Q" : null);

                                // Quantité(L)
                                AddRow("Quantité(L)", NbrLitersText);

                                // If "Montant dû" is empty and we have a portion, include Portion and Q.Livrée (produced - portion)
                                if (string.IsNullOrWhiteSpace(AmountDueText) && !string.IsNullOrWhiteSpace(PortionLitersText) && !string.IsNullOrWhiteSpace(DeliveredLitersText))
                                {
                                    AddRow("Portion(L)", $"{PortionLitersText}");
                                    AddRow("Q.Livrée(L)", $"{DeliveredLitersText}");
                                }

                                AddRow("Prix/L", UnitPriceText);
                                AddRow("Litres payés", PayedLitersText);
                                AddRow("Montant dû", AmountDueText);
                            });
                        });

                        col.Item().PaddingTop(6).Text($"Imprimé le: {DateTime.Now.ToString("f", fr)}").FontSize(9);
                        col.Item().PaddingVertical(6).LineHorizontal(1);
                        col.Item().Text("Merci pour votre confiance").AlignCenter().FontSize(9).Bold();
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
