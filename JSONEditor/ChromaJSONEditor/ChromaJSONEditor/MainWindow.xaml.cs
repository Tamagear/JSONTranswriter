using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using System.IO;
using ChromaJSONEditor.Dialogs;
using System.Windows.Documents;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace ChromaJSONEditor
{
    public partial class MainWindow : Window
    {
        private bool holdsControl = false;
        private bool holdsShift = false;
        private bool showAllAttributes = false;
        private bool hasUnsavedChanges = false;
        private int lastUsedIndex = 0;
        private string lastDisplayedBaseJSON = string.Empty;
        private List<Database> entries = new List<Database>();

        private string currentFilterName = string.Empty;
        private string currentFilterColor = string.Empty;
        private string currentFilterColorIDshortName = string.Empty;

        private string databasePath = string.Empty;
        private string basePath = string.Empty;
        private string syntaxPath = string.Empty;        
        public IHighlightingDefinition syntax { get; set; }

        private const string PATH_DATABASE_SUFFIX = @"database.json";
        private const string PATH_DATABASE_BACKUP_SUFFIX = @"database-BACKUP.json";
        private const string GENERATOR_FILE = @"D:\Projekte\Vivacious\2022\Chroma\Tools\JSONTranswriter\main.py";
        private const string START_ID = "Rx0000";
        private const string VERSION = "1.2.0";

        private string pathDatabase => databasePath + PATH_DATABASE_SUFFIX;
        private string pathDatabaseBackup => databasePath + PATH_DATABASE_BACKUP_SUFFIX;
        private string JSONEditorBoxText {
            get => JSONEditorEditorBox.Text;
            set
            {
                JSONEditorEditorBox.Text = value;
            }
        }
        private Database lastShownEntry => entries[lastUsedIndex];

        public MainWindow()
        {
            InitializeComponent();           
            SetDatabasePath();
            Startup();           
        }

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            FillListWithCurrentFilters();
            do
            {
                await Task.Delay(10);
                ShowJSONEntry(GetEntryFromID(START_ID));
            } while (JSONEditorBoxText == "Loading...");
        }

        private void Startup()
        {
            using (StreamReader r = new StreamReader(pathDatabase))
            {
                Root deserializedClass = JsonConvert.DeserializeObject<Root>(r.ReadToEnd());
                entries = deserializedClass.database;
            }

            VersionLabel.Content = VersionLabel.Content.ToString().Replace("X.Y.Z", VERSION);

            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(syntaxPath))
            {
                JSONEditorEditorBox.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
        }

        private void FillListWithCurrentFilters()
        {
            CardSearchListBox.Items.Clear();

            entries.ForEach(db => {
                if (FitsCurrentFilter(db))
                    CardSearchListBox.Items.Add($"{db.ID}\t|\t{db.name}");
            });

            ScrollToLastUsedIndex();
        }

        private void ScrollToLastUsedIndex()
        {
            if (CardSearchListBox.Items.Count > 0 && lastUsedIndex < CardSearchListBox.Items.Count)
                CardSearchListBox.ScrollIntoView(CardSearchListBox.Items[lastUsedIndex]);
        }

        private bool FitsCurrentFilter(Database entry)
        {
            if (entry == null) return false;

            return (string.IsNullOrEmpty(currentFilterName) || Contains(entry.name, currentFilterName) || Contains(entry.ID, currentFilterName))
                && (string.IsNullOrEmpty(currentFilterColor) || entry.color == currentFilterColor.ToLower() || entry.ID.StartsWith(currentFilterColorIDshortName));
        }

        private bool PartNeedsToBeDisplayed(string part, Database entry,
            ref string currentParent) 
        {
            string entryType = entry.card_type;

            if (!string.IsNullOrEmpty(currentParent))
            {
                if (part.Trim() == "],")
                    currentParent = string.Empty;

                return true;
            }

            if (part.Contains("\""))
            {
                string partName = part.Split('\"')[1];

                bool baseCondition = partName == "ID" || partName == "name" || partName == "description" || partName == "color" || partName == "card_type" || partName == "is_life_cloth";

                if (baseCondition)
                    return true;

                switch (entryType)
                {
                    case "leader":
                        if (partName == "passive_abilities" || partName == "active_abilities" || partName == "leader_attacks" || partName == "life_cloth_thresholds")
                            currentParent = partName;
                        return partName == "passive_abilities" || partName == "active_abilities" || partName == "leader_attacks" || partName == "level" || partName == "cost" || partName == "health" || partName == "life_cloth_thresholds";
                    case "unit":
                        return ((partName == "level" || partName == "cost" || partName == "throwaway_cost") && entry.is_life_cloth == "0") || partName == "space" || partName == "strength" || partName == "health";

                    case "magic":
                        return ((partName == "level" || partName == "cost" || partName == "throwaway_cost") && entry.is_life_cloth == "0") || partName == "spell_speed";
                }
            }

            return true;
        }

        private int HighestIndexForColorShort(string colorShort, out int listIndex)
        {
            int currentHighestIndex = 0;
            int highestListIndex = 0;
            foreach(Database db in entries)
            {
                if (db.ID.StartsWith($"{colorShort}x"))
                {
                    int x = int.Parse(db.ID.Split('x')[1], System.Globalization.NumberStyles.HexNumber);
                    if (x > currentHighestIndex)
                    {
                        highestListIndex = entries.IndexOf(db);
                        currentHighestIndex = x;
                    }
                }
            }

            listIndex = highestListIndex;
            return currentHighestIndex;
        }

        private void ShowJSONEntry(Database entry)
        {
            if (entry == null) { Console.WriteLine("NULL"); return; }

            hasUnsavedChanges = false;

            string result = JsonConvert.SerializeObject(entry, Formatting.Indented);
            List<string> finalParts = new List<string>();
            string[] parts = result.Split('\n');
            string currentParent = string.Empty;
            for (int i = 0; i < parts.Length; i++)
            {
                if (!showAllAttributes && !PartNeedsToBeDisplayed(parts[i], entry, ref currentParent))
                    continue;

                parts[i] = parts[i].Replace("  ", "\t");
                finalParts.Add(parts[i]);
            }
            
            result = string.Join("\n", finalParts.ToArray());

            JSONEditorBoxText = result;

            lastUsedIndex = entries.IndexOf(entry);
            lastDisplayedBaseJSON = JSONEditorBoxText;
            CardSearchListBox.SelectedIndex = lastUsedIndex;
        }

        private Database GetEntryFromID(string id)
        {
            foreach(Database db in entries)
                if(db != null && db.ID == id)
                    return db;

            return null;
        }

        private bool Contains(string source, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        private void SerializeInJSONFile()
        {
            File.Copy(pathDatabase, pathDatabaseBackup, true);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = NullValueHandling.Ignore;          
            
            Root root = new Root();

            entries.RemoveAll(item => item == null);

            root.database = entries;

            File.WriteAllText(pathDatabase, JsonConvert.SerializeObject(root, settings));

            Console.WriteLine("Gespeichert!");
        }

        private bool CheckForUnsavedChanges()
        {
            hasUnsavedChanges |= lastDisplayedBaseJSON != JSONEditorBoxText;

            if (hasUnsavedChanges)
            {
                if (!Title.EndsWith("*"))
                    Title += "*";

                MessageBoxResult result = MessageBox.Show($"Save changes to {lastShownEntry.name} (ID: {lastShownEntry.ID})?", "Unsaved changes detected!", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                    return SaveFromBox();
                else if (Title.EndsWith("*"))
                    Title = Title.Substring(0, Title.Length - 1);
            }

            return true;
        }

        private void CreateCardsFor(string color, int amount)
        {
            string colorShort = ColorToColorShort(color);
            int startIndex = HighestIndexForColorShort(colorShort, out int listIndex) + 1;
            for (int i=startIndex; i<startIndex+amount; i++)
            {
                int indx = i;
                Database entry = new Database();
                entry.ID = $"{colorShort}x{indx.ToString("X4").ToLower()}";
                entry.name = "<New Card>";
                entry.color = color;

                entries.Insert(listIndex+1+(i-startIndex), entry);
                if (i == startIndex)
                    ShowJSONEntry(entry);
            }
        }

        private string ColorToColorShort(string color)
        {
            switch(color)
            {
                case "red": return "R";
                case "purple": return "P";
                case "green": return "G";
                case "blue": return "B";
                case "black": return "S";
            }

            return null;
        }
       
        private void SetDatabasePath()
        {
            string cur = Directory.GetCurrentDirectory();
            while(!string.IsNullOrEmpty(cur) && !cur.EndsWith(@"JSONTranswriter"))
            {
                //Console.WriteLine("Currently in: " + cur);
                cur = Directory.GetParent(cur)?.ToString();
            }

            if (string.IsNullOrEmpty(cur))
                MessageBox.Show("Please execute this program from within (a subfolder of) this project's repository.", "Error: Default Database Path not found!", MessageBoxButton.OK, MessageBoxImage.Error);

            basePath = cur;
            syntaxPath = $@"{cur}\Syntax.xshd";
            databasePath = $@"{cur}\content\database\";
        }

        private void ShowNext()
        {
            int targetIndex = lastUsedIndex + 1;
            if (targetIndex >= entries.Count)
                targetIndex = 0;

            ShowJSONEntry(entries[targetIndex]);
        }

        private void ShowPrevious()
        {
            int targetIndex = lastUsedIndex - 1;
            if (targetIndex < 0)
                targetIndex = entries.Count-1;

            ShowJSONEntry(entries[targetIndex]);
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
                            SaveFromBox();
                    break;
                case Key.Delete:
                    Delete_Current_Card_Click(sender, null);
                    break;
                case Key.N:
                    if (holdsControl)
                        New_Card_Click(sender, null);
                    break;
                case Key.PageUp:
                    ShowPrevious();
                    break;
                case Key.PageDown:
                    ShowNext();
                    break;
                case Key.Left:
                    ShowPrevious();
                    break;
                case Key.Right:
                    ShowNext();
                    break;
                case Key.Up:
                    ShowPrevious();
                    break;
                case Key.Down:
                    ShowNext();
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
            SaveFromBox();
        }

        private bool SaveFromBox()
        {
            lastDisplayedBaseJSON = JSONEditorBoxText;

            hasUnsavedChanges = false;
            if (Title.EndsWith("*"))
                Title = Title.Substring(0, Title.Length - 1);

            try
            {
                entries[lastUsedIndex] = JsonConvert.DeserializeObject<Database>(JSONEditorBoxText);

                FillListWithCurrentFilters();

                SerializeInJSONFile();
                return true;
            } catch (JsonReaderException exception)
            {
                MessageBox.Show($"Upon saving, an error occured:\n{exception.Message}\n\nPlease remember to check if your syntax is correct! Especially when it comes to using the right brackets.\n\nHelp can be found here: https://www.w3resource.com/JSON/structures.php", "Reader Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void Save_All_Click(object sender, RoutedEventArgs e)
        {
            SaveFromBox();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Lade-Dialog
        }

        private void New_Card_Click(object sender, RoutedEventArgs e)
        {
            NewCardDialog ncd = new NewCardDialog();
            ncd.Show();
            GetWindow(ncd).Closing += (s, x) =>
            {
                NewCardResponse response = ncd.response;
                if (response != null && response.ok)
                {
                    CreateCardsFor(response.color, response.amount);

                    FillListWithCurrentFilters();
                    ScrollToLastUsedIndex();

                    SerializeInJSONFile();
                }
            };
        }

        private void Delete_Current_Card_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {lastShownEntry.name} (ID: {lastShownEntry.ID})?\n\nTHIS CAN NOT BE UNDONE!", "Really delete card?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Database deleteEntry = lastShownEntry;
                entries.Remove(deleteEntry);

                FillListWithCurrentFilters();

                SerializeInJSONFile();

                ShowJSONEntry(entries[0]);
            }
        }

        private void GenerateDeck_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Execute main.py in root!", "Function not implemented yet", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ListBox_SelectionChanged(object sender, EventArgs e)
        {
            if (sender != null)
            {
                ListBox selected = (ListBox)sender;

                bool success = CheckForUnsavedChanges();

                if (success)
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

        private void JSONEditorEditorBox_TextChanged(object sender, EventArgs e)
        {
            hasUnsavedChanges = true;

            if (!Title.EndsWith("*"))
                Title += "*";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CheckForUnsavedChanges();
        }
    }
}