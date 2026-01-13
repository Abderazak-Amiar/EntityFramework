using System;
using System.Collections.Generic;
using System.Globalization;

namespace EntityFramework
{
    /// <summary>
    /// Simple localization map for UI strings (English + French).
    /// Uses CurrentUICulture to pick language (any code starting with "fr" => French, otherwise English).
    /// Add keys as needed.
    /// </summary>
    public static class AppStrings
    {
        private static readonly Dictionary<string, (string en, string fr)> _map = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Info"] = ("Info","Info"),
            ["Error"] = ("Error","Erreur"),
            ["ParamsNotFound"] = ("No Parameters record found in the database. Create a row with Id=1 or initialize the table.", "Aucun enregistrement de paramètres trouvé dans la base de données. Créez d'abord un enregistrement (Id=1) ou initialisez la table."),
            ["ParamsSaved"] = ("Parameters saved and reloaded.", "Paramètres sauvegardés et rechargés."),
            ["ErrorSavingParams"] = ("Error saving parameters: {0}", "Erreur lors de l'enregistrement des paramètres : {0}"),
            ["VenteSavedPrinted"] = ("Sale recorded and print requested.", "Vente enregistrée et impression demandée."),
            ["VenteSaved"] = ("Sale recorded.", "Vente enregistrée."),
            ["VenteLitersInvalid"] = ("Invalid number of liters.", "Nombre de litres invalide."),
            ["VentePriceInvalid"] = ("Invalid price.", "Prix invalide."),
            ["DbSetVentesNull"] = ("Database context is not correctly configured. The DbSet 'Ventes' is null.", "Le contexte de la base de données n'est pas correctement configuré. Le DbSet 'Ventes' est nul."),
            ["DeleteVenteConfirm"] = ("Delete this sale?","Supprimer cette vente ?"),
            ["DeleteVenteTitle"] = ("Confirm deletion", "Confirmer la suppression"),
            ["ErrorDeletingVente"] = ("Error deleting sale: {0}", "Erreur lors de la suppression de la vente : {0}"),
            ["SelectUserToPrint"] = ("Select a user to print.", "Sélectionnez un utilisateur à imprimer."),
            ["SelectedRowNotUser"] = ("The selected row is not a user.", "La ligne sélectionnée n'est pas un utilisateur."),
            ["PrintFailed"] = ("Printing failed: {0}", "Échec de l'impression : {0}"),
            ["UserNameAddressRequired"] = ("Name and address are required.", "Le nom et l'adresse sont requis."),
            ["DbSetUsersNull"] = ("Database context is not correctly configured. The DbSet 'Users' is null.", "Le contexte de la base de données n'est pas correctement configuré. Le DbSet 'Users' est nul."),
            ["UserCreated"] = ("User created successfully", "Utilisateur créé avec succès"),
            ["UserCreateError"] = ("Error creating user: {0}", "Erreur lors de la création: {0}"),
            ["NoUserSelected"] = ("No user selected.", "Aucun utilisateur sélectionné."),
            ["UserUpdated"] = ("User updated successfully", "Utilisateur mis à jour avec succès"),
            ["UserUpdateError"] = ("Error updating user: {0}", "Erreur lors de la mise à jour : {0}"),
            ["UserNotFound"] = ("The selected user was not found in the database.", "L'utilisateur sélectionné est introuvable dans la base de données."),
            ["UserDeleted"] = ("User deleted successfully", "Utilisateur supprimé avec succès"),
            ["UserDeleteError"] = ("Error deleting user: {0}", "Erreur lors de la suppression : {0}"),
            ["StatsLoadFailed"] = ("Failed loading statistics: {0}", "Échec du chargement des statistiques : {0}"),
            ["GestionClients"] = ("CLIENT MANAGEMENT","GESTION CLIENTS"),
            ["VentePageLabel"] = ("Page {0} / {1} - {2} Sales","Page {0} / {1} - {2} Ventes"),
            // grid / column labels
            ["Name"] = ("Name","Nom"),
            ["Tel"] = ("Tel","Téléphone"),
            ["Address"] = ("Address","Adresse"),
            ["TotalClients"] = ("Total Clients","Total Clients"),
            ["BagsIn"] = ("Bags in","Sacs entrée"),
            ["Weight"] = ("Weight","Poids"),
            ["LitresProduced"] = ("Litres produced","Litres produites"),
            ["PortionSoldLitres"] = ("Portion litres sold","Nombre Litres Portion vendues"),
            ["PortionEnteredLitres"] = ("Portion litres entered","Total Nombre de litre Portion Entrées"),
            ["TotalDeliveredLitres"] = ("Total delivered litres","Total Nombre de litre livrées"),
            ["Revenue"] = ("Revenue (sold litres)","Recette (litres vendues)"),
            ["TotalOilSold"] = ("Total oil sold","Total huile vendu")
        };

        private static bool IsFrench()
        {
            try
            {
                var code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                return code != null && code.Equals("fr", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static string T(string key)
        {
            if (key == null) return string.Empty;
            if (_map.TryGetValue(key, out var pair))
            {
                return IsFrench() ? pair.fr : pair.en;
            }
            return key; // fallback to key itself
        }

        public static string Tf(string key, params object[] args)
        {
            var fmt = T(key);
            try
            {
                return string.Format(CultureInfo.CurrentCulture, fmt, args);
            }
            catch
            {
                return fmt;
            }
        }
    }
}
