using ReminderApplication.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ReminderApplication.UserControls
{
    public partial class Item : UserControl
    {
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register("Note", typeof(Note), typeof(Item));
        public event EventHandler<Note> OnNoteDeleted;
        public event EventHandler<Note> NoteChanged;
        public Note Note
        {
            get { return (Note)GetValue(NoteProperty); }
            set { SetValue(NoteProperty, value); }
        }
        public Item()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(Item));
        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }
        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(string), typeof(Item));
        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(Item));
        public FontAwesome.WPF.FontAwesomeIcon Icon
        {
            get { return (FontAwesome.WPF.FontAwesomeIcon)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(FontAwesome.WPF.FontAwesomeIcon), typeof(Item));

        public FontAwesome.WPF.FontAwesomeIcon IconBell
        {
            get { return (FontAwesome.WPF.FontAwesomeIcon)GetValue(IconBellProperty); }
            set { SetValue(IconBellProperty, value); }
        }
        public static readonly DependencyProperty IconBellProperty = DependencyProperty.Register("IconBell", typeof(FontAwesome.WPF.FontAwesomeIcon), typeof(Item));

        private void ToggleIsChecked()
        {
            if (Note == null) return;
            Note.IsCompleted = !Note.IsCompleted;
            Note.ColorHex = Note.IsCompleted ? "#eba5bb" : "#f1f1f1";
            Icon = Note.IsCompleted ? FontAwesome.WPF.FontAwesomeIcon.CheckCircle : FontAwesome.WPF.FontAwesomeIcon.CircleThin;
            Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Note.IsCompleted ? "#eba5bb" : "#f1f1f1"));
            NoteChanged?.Invoke(this, Note);
        }
        private void ClickOnBell_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ToggleIsChecked();

        private void MenuButtonDelete_ClickPreview(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Note != null && this.Parent is Panel parent)
            {
                parent.Children.Remove(this);
                new NotesFileService().DeleteNote(Note);
                OnNoteDeleted?.Invoke(this, Note);
            }
            btnMenu.IsChecked = false;
        }

        private void MenuButtonMute_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Note == null) return;
            Note.HasReminder = !Note.HasReminder;
            IconBellToggle.Icon = Note.HasReminder ? FontAwesome.WPF.FontAwesomeIcon.Bell : FontAwesome.WPF.FontAwesomeIcon.BellSlash;
            NoteChanged?.Invoke(this, Note);
        }

        private void MenuButtonCheck_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ToggleIsChecked();
            btnMenu.IsChecked = false;
            NoteChanged?.Invoke(this, Note);
        }
    }
}
