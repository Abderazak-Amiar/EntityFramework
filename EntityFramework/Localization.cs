using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace EntityFramework
{
    /// <summary>
    /// Small runtime localization helper:
    /// - sets application's culture from Parameters.LanguageCode
    /// - applies resources to a Form and its controls (handles RTL switch)
    /// Only English and French are supported; any other value falls back to English.
    /// </summary>
    public static class Localization
    {
        /// <summary>
        /// Read saved language from DB (Parameters.Id==1) and apply it to the current process/thread.
        /// Safe for first-run (falls back to "en").
        /// </summary>
        //public static void SetAppCultureFromParameters()
        //{
        //    string lang = "en";
        //    try
        //    {
        //        using var ctx = new DataContext();
        //        var p = ctx.Parameters?.FirstOrDefault(x => x.Id == 1);
        //        if (p != null && !string.IsNullOrWhiteSpace(p.LanguageCode))
        //            lang = p.LanguageCode.Trim().ToLowerInvariant();
        //    }
        //    catch
        //    {
        //        // swallow DB errors and fallback to default
        //        lang = "en";
        //    }
        //    SetCulture(lang);
        //}

        /// <summary>
        /// Map a short language code ("en", "fr", or full culture) to a CultureInfo and apply it globally.
        /// Only "en" and "fr" are accepted; other values will fall back to "en-US".
        /// </summary>
        public static void SetCulture(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode)) languageCode = "en";
            CultureInfo ci;
            try
            {
                var code = languageCode.Trim().ToLowerInvariant();

                if (code.StartsWith("fr"))
                {
                    ci = new CultureInfo("fr-FR");
                }
                else if (code.StartsWith("en"))
                {
                    ci = new CultureInfo("en-US");
                }
                else
                {
                    // Disallow other languages — default to en-US
                    ci = new CultureInfo("en-US");
                }
            }
            catch
            {
                ci = new CultureInfo("en-US");
            }

            // apply to current and future threads
            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        /// <summary>
        /// Apply resources (from .resx) to the provided form and all child controls.
        /// Also toggles RightToLeft / RightToLeftLayout when the culture is RTL.
        /// Use this after changing CurrentUICulture to refresh visible UI strings.
        /// </summary>
        public static void ApplyToForm(Form form)
        {
            if (form == null) return;

            var ci = CultureInfo.CurrentUICulture;
            var isRtl = ci.TextInfo.IsRightToLeft;

            try
            {
                var rm = new ComponentResourceManager(form.GetType());

                // Apply resources to the form itself ("$this") then recursively to children
                try { rm.ApplyResources(form, "$this"); } catch { /* best-effort */ }

                ApplyResourcesRecursive(rm, form);

                // ToolStrip / MenuStrip items are not Controls so handle them explicitly
                if (form.MainMenuStrip != null)
                    ApplyToolStripItemsResources(rm, form.MainMenuStrip.Items);

                // Set RTL layout properties if necessary
                form.RightToLeft = isRtl ? RightToLeft.Yes : RightToLeft.No;
                form.RightToLeftLayout = isRtl;
            }
            catch
            {
                // swallow - do not break the UI if resources fail
            }
        }

        private static void ApplyResourcesRecursive(ComponentResourceManager rm, Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                try { rm.ApplyResources(c, c.Name); } catch { }
                // Recurse
                if (c.HasChildren) ApplyResourcesRecursive(rm, c);

                // ToolStrip children that are Controls (e.g., ToolStripContainer) may contain ToolStrip
                if (c is ToolStripContainer tsc)
                {
                    if (tsc.ContentPanel != null) ApplyResourcesRecursive(rm, tsc.ContentPanel);
                    if (tsc.TopToolStripPanel != null) ApplyResourcesRecursive(rm, tsc.TopToolStripPanel);
                }

                // If the control hosts a ToolStrip (common in WinForms designers)
                if (c is ToolStrip toolStrip)
                {
                    ApplyToolStripItemsResources(rm, toolStrip.Items);
                }
            }
        }

        private static void ApplyToolStripItemsResources(ComponentResourceManager rm, ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                if (!string.IsNullOrEmpty(item.Name))
                {
                    try { rm.ApplyResources(item, item.Name); } catch { }
                }
                if (item is ToolStripMenuItem menuItem && menuItem.DropDownItems != null && menuItem.DropDownItems.Count > 0)
                {
                    ApplyToolStripItemsResources(rm, menuItem.DropDownItems);
                }
            }
        }
    }
}