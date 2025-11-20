using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Linq;
using TSMapEditor.Models;
using TSMapEditor.UI;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class AITargetTypesWindow : INItializableWindow
    {
        public AITargetTypesWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        private EditorListBox lbAITargetTypes;
        private EditorListBox lbTechnoEntries;
        private EditorListBox lbAvailableTechnos;
        private EditorSuggestionTextBox tbSearchTechno;

        private readonly List<GameObjectType> allAvailableTechnos = new List<GameObjectType>();
        private int editedIndex = -1;

        public override void Initialize()
        {
            Name = nameof(AITargetTypesWindow);
            base.Initialize();

            lbAITargetTypes = FindChild<EditorListBox>(nameof(lbAITargetTypes));
            lbTechnoEntries = FindChild<EditorListBox>(nameof(lbTechnoEntries));
            lbAvailableTechnos = FindChild<EditorListBox>(nameof(lbAvailableTechnos));
            tbSearchTechno = FindChild<EditorSuggestionTextBox>(nameof(tbSearchTechno));

            UIHelpers.AddSearchTipsBoxToControl(tbSearchTechno);

            FindChild<EditorButton>("btnNewAITargetType").LeftClick += BtnNewAITargetType_LeftClick;
            FindChild<EditorButton>("btnDeleteAITargetType").LeftClick += BtnDeleteAITargetType_LeftClick;
            FindChild<EditorButton>("btnCloneAITargetType").LeftClick += BtnCloneAITargetType_LeftClick;
            FindChild<EditorButton>("btnAddTechno").LeftClick += BtnAddTechno_LeftClick;
            FindChild<EditorButton>("btnDeleteTechno").LeftClick += BtnDeleteTechno_LeftClick;

            lbAITargetTypes.SelectedIndexChanged += LbAITargetTypes_SelectedIndexChanged;

            tbSearchTechno.TextChanged += (s, e) => ApplyTechnoFilter();

            ListAvailableTechnos();
            ListAITargetTypes();
        }

        public void Open()
        {
            ListAvailableTechnos();
            ListAITargetTypes();
            Show();
        }

        private void ListAITargetTypes()
        {
            lbAITargetTypes.Clear();

            for (int i = 0; i < map.AITargetTypes.Count; i++)
            {
                var aiTargetType = map.AITargetTypes[i];
                string value = aiTargetType.WriteToIniString();
                lbAITargetTypes.AddItem(new XNAListBoxItem { Text = $"{i}: {value}", Tag = i });
            }
        }

        private void LbAITargetTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbAITargetTypes.SelectedItem == null)
            {
                editedIndex = -1;
                lbTechnoEntries.Clear();
                return;
            }

            editedIndex = (int)lbAITargetTypes.SelectedItem.Tag;

            var aiTargetType = map.AITargetTypes[editedIndex];
            PopulateTechnoEntries(aiTargetType.TechnoNames);
        }

        private void PopulateTechnoEntries(List<string> technoNames)
        {
            lbTechnoEntries.Clear();
            foreach (var technoName in technoNames)
            {
                var trimmed = technoName.Trim();
                var match = allAvailableTechnos.FirstOrDefault(t => string.Equals(t.ININame, trimmed, StringComparison.CurrentCultureIgnoreCase));
                lbTechnoEntries.AddItem(new XNAListBoxItem { Text = trimmed, Tag = (object)match ?? trimmed });
            }
        }

        private void ListAvailableTechnos()
        {
            allAvailableTechnos.Clear();
            allAvailableTechnos.AddRange(map.Rules.UnitTypes);
            allAvailableTechnos.AddRange(map.Rules.InfantryTypes);
            allAvailableTechnos.AddRange(map.Rules.BuildingTypes);
            allAvailableTechnos.AddRange(map.Rules.AircraftTypes);
            allAvailableTechnos.Sort((a, b) => string.Compare(a.ININame, b.ININame, StringComparison.OrdinalIgnoreCase));

            ApplyTechnoFilter();
        }

        private void ApplyTechnoFilter()
        {
            if (lbAvailableTechnos == null)
                return;

            lbAvailableTechnos.Clear();

            string filter = tbSearchTechno?.Text?.Trim() ?? string.Empty;
            bool showAll = string.IsNullOrWhiteSpace(filter) || filter.Equals(tbSearchTechno.Suggestion, StringComparison.CurrentCultureIgnoreCase);

            foreach (var techno in allAvailableTechnos)
            {
                if (!showAll && !techno.ININame.Contains(filter, StringComparison.CurrentCultureIgnoreCase) && 
                    !techno.GetEditorDisplayName().Contains(filter, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                lbAvailableTechnos.AddItem(new XNAListBoxItem { Text = $"{techno.ININame} ({techno.GetEditorDisplayName()})", Tag = techno });
            }
        }

        private void BtnNewAITargetType_LeftClick(object sender, EventArgs e)
        {
            var aiTargetType = new AITargetType();
            aiTargetType.TechnoNames.Add("E1");
            map.AITargetTypes.Add(aiTargetType);

            ListAITargetTypes();
            lbAITargetTypes.SelectedIndex = lbAITargetTypes.Items.Count - 1;
        }

        private void BtnDeleteAITargetType_LeftClick(object sender, EventArgs e)
        {
            if (editedIndex == -1)
                return;

            map.AITargetTypes.RemoveAt(editedIndex);

            ListAITargetTypes();
            if (lbAITargetTypes.Items.Count == 0)
                editedIndex = -1;
            else
                lbAITargetTypes.SelectedIndex = Math.Min(editedIndex, lbAITargetTypes.Items.Count - 1);
        }

        private void BtnCloneAITargetType_LeftClick(object sender, EventArgs e)
        {
            if (editedIndex == -1)
                return;

            var original = map.AITargetTypes[editedIndex];
            var clone = new AITargetType(new List<string>(original.TechnoNames));
            map.AITargetTypes.Add(clone);

            ListAITargetTypes();
            editedIndex = map.AITargetTypes.Count - 1;
            SelectCurrentEntry();
        }

        private void BtnAddTechno_LeftClick(object sender, EventArgs e)
        {
            if (editedIndex == -1 || lbAvailableTechnos.SelectedItem == null)
                return;

            var techno = (GameObjectType)lbAvailableTechnos.SelectedItem.Tag;
            lbTechnoEntries.AddItem(new XNAListBoxItem { Text = techno.ININame, Tag = techno });
            CommitTechnoEntries();
        }

        private void BtnDeleteTechno_LeftClick(object sender, EventArgs e)
        {
            if (editedIndex == -1 || lbTechnoEntries.SelectedItem == null)
                return;

            int selected = lbTechnoEntries.SelectedIndex;
            var remaining = lbTechnoEntries.Items.Where((item, index) => index != selected).Select(item => item.Text).ToList();

            lbTechnoEntries.Clear();
            foreach (var entry in remaining)
                lbTechnoEntries.AddItem(new XNAListBoxItem { Text = entry, Tag = entry });

            CommitTechnoEntries();
            lbTechnoEntries.SelectedIndex = Math.Max(0, selected - 1);
        }

        private void CommitTechnoEntries()
        {
            if (editedIndex == -1)
                return;

            var aiTargetType = map.AITargetTypes[editedIndex];
            aiTargetType.TechnoNames.Clear();
            aiTargetType.TechnoNames.AddRange(lbTechnoEntries.Items.Select(item => item.Text).Where(text => !string.IsNullOrWhiteSpace(text)));

            ListAITargetTypes();
            SelectCurrentEntry();
        }

        private void SelectCurrentEntry()
        {
            for (int i = 0; i < lbAITargetTypes.Items.Count; i++)
            {
                if ((int)lbAITargetTypes.Items[i].Tag == editedIndex)
                {
                    lbAITargetTypes.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}