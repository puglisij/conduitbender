using UnityEngine;
using UnityEngine.UI;

namespace CB
{
    public class DialogModal : AModal
    {
        public enum EDialogSelection { Unselected, Yes, No, Cancel }

        public delegate void OnSelection( EDialogSelection selection );

        public OnSelection onSelection;

        public Text headerText;

        public RectTransform scrollContent;
        public Button yesButton;
        public Button noButton;

        public EDialogSelection selection { get { return m_selection; } }
        EDialogSelection m_selection = EDialogSelection.Unselected;

        protected override void Awake()
        {
            base.Awake();

            yesButton.onClick.AddListener( OnYes );
            noButton.onClick.AddListener( OnNo );
        }

        public void OnYes()
        {
            m_selection = EDialogSelection.Yes;
            onSelection( m_selection );
        }
        public void OnNo()
        {
            m_selection = EDialogSelection.No;
            onSelection( m_selection );
        }

        public override string modalTitle {
            get { return m_ModalTitle; }
            set {
                m_ModalTitle = value;
                headerText.text = m_ModalTitle;
            }
        }

    }
}
