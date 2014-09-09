using MarkdownSharp;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace WindowsMarkdownEditor
{
    public partial class MainWindow : Window
    {
        Markdown md = new Markdown();
        string filename;
        static string default_text = "<html><h1>Windows Markdown Editor</h1><p><a href=\"http://daringfireball.net/projects/markdown/syntax\">Markdown Syntax</a></html>";
        static string filefilter = "Text Files(*.txt, *.md)|*.txt;*.md|All(*.*)|*";

        public MainWindow()
        {
            InitializeComponent();
            Preview.NavigateToString(default_text);
        }

        private void UpdatePreview()
        {
            if (!string.IsNullOrWhiteSpace(Source.Text))
                Preview.NavigateToString(md.Transform(Source.Text));
            else
                Preview.NavigateToString(default_text);
        }

        private void KeyUp_Handler(object sender, KeyEventArgs e)
        {
            UpdatePreview();
        }

        private void SizeChanged_Handler(object sender, SizeChangedEventArgs e)
        {
            Resize();
        }

        private void Resize()
        {
            // rough resize, not sure how to do this auto
            Source.Height = this.ActualHeight - 50;
            Source.Width = this.ActualWidth / 2 - 22;
            Preview.Margin = new System.Windows.Thickness() { Left = this.ActualWidth / 2 };
            Preview.Height = this.ActualHeight - 50;
            Preview.Width = this.ActualWidth / 2 - 22;
        }

        private void StateChanged_Handler(object sender, EventArgs e)
        {
            // Since maximize a window doesn't trigger SizeChanged we need to check for that state
            // manually and and resize.
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    Resize();
                    break;
            }
        }

        private void LoadCompleted_Handler(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Scroll preview to approximate position. This not very accurate.
            mshtml.HTMLDocument htmlDoc = Preview.Document as mshtml.HTMLDocument;
            int offset = (int)((Source.VerticalOffset/Source.Height)*Preview.Height);
            if (htmlDoc != null) htmlDoc.parentWindow.scrollTo(0, offset);
        }

        #region Context Menu
        private void ClickPaste(Object sender, RoutedEventArgs args) { Source.Paste(); UpdatePreview(); }
        private void ClickCopy(Object sender, RoutedEventArgs args) { Source.Copy(); }
        private void ClickCut(Object sender, RoutedEventArgs args) { Source.Cut(); UpdatePreview(); }
        private void ClickSelectAll(Object sender, RoutedEventArgs args) { Source.SelectAll(); }
        private void ClickClear(Object sender, RoutedEventArgs args) { Source.Clear(); UpdatePreview(); }
        private void ClickUndo(Object sender, RoutedEventArgs args) { Source.Undo(); UpdatePreview(); }
        private void ClickRedo(Object sender, RoutedEventArgs args) { Source.Redo(); UpdatePreview(); }

        private void ClickSaveAs(object sender, RoutedEventArgs e)
        {
            string fileText = Source.Text;

            var dialog = new SaveFileDialog() { Filter = filefilter };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, fileText);
                filename = dialog.FileName;
            }
        }

        private void ClickSave(object sender, RoutedEventArgs e)
        {            
            if (string.IsNullOrWhiteSpace(filename))
                ClickSaveAs(sender, e);
            else
                File.WriteAllText(filename, Source.Text);
        }

        private void ClickLoad(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog() { Filter = filefilter };

            if (dialog.ShowDialog() == true)
            {
                Source.Text = File.ReadAllText(dialog.FileName);
                UpdatePreview();
                filename = dialog.FileName;
            }
        }

        private void CxmOpened(object sender, RoutedEventArgs e)
        {
            // Only allow copy/cut if something is selected to copy/cut. 
            if (Source.SelectedText == "")
                cxmItemCopy.IsEnabled = cxmItemCut.IsEnabled = false;
            else
                cxmItemCopy.IsEnabled = cxmItemCut.IsEnabled = true;

            // Only allow paste if there is text on the clipboard to paste. 
            if (Clipboard.ContainsText())
                cxmItemPaste.IsEnabled = true;
            else
                cxmItemPaste.IsEnabled = false;

            if (string.IsNullOrWhiteSpace(Source.Text))
                cxmItemSave.IsEnabled = cxmItemSaveAs.IsEnabled = false;
            else
                cxmItemSave.IsEnabled = cxmItemSaveAs.IsEnabled = true;
        }
        #endregion Context Menu

    }
}
