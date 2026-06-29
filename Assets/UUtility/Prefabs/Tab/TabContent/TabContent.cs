using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UTool.TabSystem
{
    public class TabContent : MonoBehaviour
    {
        [SerializeField] private Transform contentHolder;
        [SerializeField] private TextMeshProUGUI nameDisplay;
        [SpaceArea]
        [SerializeField][ReorderableList(Foldable = true)] private List<VariableCtrl> variableTypesPrefab = new List<VariableCtrl>();

        private List<VariableCtrl> variableTypes = new List<VariableCtrl>();

        private Tab tab;

        public void BindToTab(Tab tab)
        {
            this.tab = tab;
            tab.content = this;

            nameDisplay.text = tab.tabName;
        }

        public void AddTVariableControllers()
        {
            foreach (TVariable tVariable in tab.tabVariables)
            {
                tVariable.StoreVectorData();

                VariableCtrl variableCtrl = Instantiate(variableTypesPrefab[(int)tVariable.variableType], contentHolder);
                variableCtrl.AssignVariable(tVariable);

                variableTypes.Add(variableCtrl);
            }
        }

        public void ApplyDefault()
        {
            tab.ApplyDefaultValue();
            UpdateVariableDisplay();

            ApplyChanges();
        }

        public void UpdateVariableDisplay()
        {
            foreach (VariableCtrl vCtrl in variableTypes)
                vCtrl.UpdateDisplay();
        }

        public void ApplyChanges()
        {
            tab.ApplyTabVariables();
        }
    }
}