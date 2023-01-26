using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace ChromaJSONEditor
{
    public partial class MainWindow : Window
    {
        private bool holdsControl = false;
        private bool holdsShift = false;
        private bool showAllAttributes = false;
        private string lastShownJSON = string.Empty;
        private List<Database> entries = new List<Database>();
        private Database lastShownEntry = null;

        private string newJson = string.Empty; // TODO: Füllen und Abspeichern

        private string currentFilterName = string.Empty;
        private string currentFilterColor = string.Empty;
        private string currentFilterColorIDshortName = string.Empty;

        private const string PATH_DATABASE = @"D:\Projekte\Vivacious\2022\Chroma\Tools\JSONTranswriter\content\database\database.json";
        private const string GENERATOR_FILE = @"D:\Projekte\Vivacious\2022\Chroma\Tools\JSONTranswriter\main.py";
        private const string START_ID = "Rx0000";
        private const string VERSION = "1.0.0";
        private const bool PATH_STARTS_IN_PARENT = true;

        public MainWindow()
        {
            InitializeComponent();
            Startup();           
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            FillListWithCurrentFilters();
            ShowJSONEntry(GetEntryFromID(START_ID));
        }

        private void Startup()
        {
            using (StreamReader r = new StreamReader(PATH_DATABASE))
            {
                Root deserializedClass = JsonConvert.DeserializeObject<Root>(r.ReadToEnd());
                entries = deserializedClass.database;
            }

            VersionLabel.Content = VersionLabel.Content.ToString().Replace("X.Y.Z", VERSION);
        }

        private void FillListWithCurrentFilters()
        {
            CardSearchListBox.Items.Clear();

            entries.ForEach(db => {
                if (FitsCurrentFilter(db))
                    CardSearchListBox.Items.Add($"{db.ID}\t|\t{db.name}");
            });

            if (CardSearchListBox.Items.Count > 0)
                CardSearchListBox.ScrollIntoView(CardSearchListBox.Items[0]);
        }

        private bool FitsCurrentFilter(Database entry)
        {
            return (string.IsNullOrEmpty(currentFilterName) || Contains(entry.name, currentFilterName) || Contains(entry.ID, currentFilterName))
                && (string.IsNullOrEmpty(currentFilterColor) || entry.color == currentFilterColor.ToLower() || entry.ID.StartsWith(currentFilterColorIDshortName));
        }

        private bool PartNeedsToBeDisplayed(string part, Database entry) //TODO: Anzeigen der Werte je nach Kartentyp ODER falls was anderes als null drinsteht ODER ShowAllAttributes
        {
            if (part.Contains("\""))
            {
                string entryType = entry.card_type;
                string partName = part.Split('\"')[1];
                bool baseCondition = partName == "ID" || partName == "name" || partName == "description" || partName == "color" || partName == "card_type" || partName == "is_life_cloth";

                if (baseCondition)
                    return true;

                switch(entryType)
                {
                    case "leader":
                        return partName == "passive_abilities" || partName == "active_abilities" || partName == "leader_attacks" || partName == "level" || partName == "cost" || partName == "health" || partName == "life_cloth_threshold";

                    case "unit":
                        return ((partName == "level" || partName == "cost" || partName == "throwaway_cost") && entry.is_life_cloth == "0") || partName == "space" || partName == "strength" || partName == "health";

                    case "magic":
                        return ((partName == "level" || partName == "cost" || partName == "throwaway_cost") && entry.is_life_cloth == "0") || partName == "spell_speed";
                }

            }

            return true;
        }

        private void ShowJSONEntry(Database entry)
        {
            if (entry == null) return;

            string result = JsonConvert.SerializeObject(entry, Formatting.Indented);
            lastShownJSON = result;
            List<string> finalParts = new List<string>();
            string[] parts = result.Split('\n');
            for (int i = 0; i < parts.Length; i++)
            {
                if (!showAllAttributes && !PartNeedsToBeDisplayed(parts[i], entry))
                    continue;

                parts[i] = parts[i].Replace("  ", "\t");
                finalParts.Add(parts[i]);
            }
            
            result = string.Join("\n", finalParts.ToArray());

            JSONEditorEditorBox.Text = result;

            lastShownEntry = entry;
        }

        private Database GetEntryFromID(string id)
        {
            foreach(Database db in entries)
                if(db.ID == id)
                    return db;

            return null;
        }

        public static bool Contains(string source, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        #region JSON Classes
        public class ActiveAbility
        {
            public string loyalty_cost { get; set; }
            public string mana_cost { get; set; }
            public string name { get; set; }
            public string description { get; set; }
        }

        public class Database
        {
            public string ID { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public List<string> passive_abilities { get; set; }
            public List<ActiveAbility> active_abilities { get; set; }
            public List<LeaderAttack> leader_attacks { get; set; }
            public string color { get; set; }
            public string level { get; set; }
            public string card_type { get; set; }
            public string cost { get; set; }
            public string throwaway_cost { get; set; }
            public string space { get; set; }
            public string strength { get; set; }
            public string health { get; set; }
            public List<string> life_cloth_thresholds { get; set; }
            public string spell_speed { get; set; }
            public string is_life_cloth { get; set; }
        }

        public class LeaderAttack
        {
            public string loyalty_cost { get; set; }
            public string mana_cost { get; set; }
            public string strength { get; set; }
            public string name { get; set; }
            public string description { get; set; }
        }

        public class Root
        {
            public List<Database> database { get; set; }
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var window = GetWindow(this);
            window.KeyDown += HandleKeyPress;
            window.KeyUp += HandleKeyUp;
        }

        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                holdsControl = false;
            else if (e.Key == Key.LeftShift)
                holdsShift = false;
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.LeftCtrl:
                    holdsControl = true;
                    break;
                case Key.LeftShift:
                    holdsShift = true;
                    break;
                case Key.S:
                    if (holdsControl)
                        if (holdsShift)
                            Save_All_Click(sender, null);
                        else
                            Save_Current_Card_Click(sender, null);
                    break;
            }
        }

        private void Search_Card_Click(object sender, RoutedEventArgs e)
        {
            currentFilterName = CardSearchTextBox.Text;
            FillListWithCurrentFilters();
        }

        private void Save_Current_Card_Click(object sender, RoutedEventArgs e)
        {
            newJson.Replace(lastShownJSON, JSONEditorEditorBox.Text); //TODO: EditorBox-Text an Json anpassen; Null-Werte ignorieren           
        }

        private void Save_All_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Liste an ungespeicherten Änderungen im Hinterkopf behalten
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Lade-Dialog
        }

        private void New_Card_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Neue Karten
        }

        private void Delete_Current_Card_Click(object sender, RoutedEventArgs e)
        {
            newJson.Replace(lastShownJSON, string.Empty); // TODO: Siehe Save
        }

        private void GenerateDeck_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Execute main.py in root!", "Function not implemented yet", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ListBox_SelectionChanged(object sender, EventArgs e)
        {
            if (sender != null)
            {
                ListBox selected = (ListBox)sender;
                ShowJSONEntry(GetEntryFromID(selected.SelectedItem?.ToString().Split('\t')[0]));
            }
        }

        private void ShowAllAttributes_Checked(object sender, EventArgs e)
        {
            showAllAttributes = true;
            ShowJSONEntry(lastShownEntry);
            ShowAttributeToggleButton.Content = "Hide All Attributes";
        }

        private void ShowAllAttributes_Unchecked(object sender, EventArgs e)
        {
            showAllAttributes = false;
            ShowJSONEntry(lastShownEntry);
            ShowAttributeToggleButton.Content = "Show All Attributes";
        }

        private void ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            currentFilterColor = ((ComboBox)sender).Text.ToString();

            switch(currentFilterColor)
            {
                case "All Colors":
                    currentFilterColor = string.Empty;
                    currentFilterColorIDshortName = string.Empty;
                    break;
                case "Red":
                    currentFilterColorIDshortName = "R";
                    break;
                case "Purple":
                    currentFilterColorIDshortName = "P";
                    break;
                case "Green":
                    currentFilterColorIDshortName = "G";
                    break;
                case "Blue":
                    currentFilterColorIDshortName = "B";
                    break;
                case "Black":
                    currentFilterColorIDshortName = "S";
                    break;
            }

            FillListWithCurrentFilters();
        }
    }
}
