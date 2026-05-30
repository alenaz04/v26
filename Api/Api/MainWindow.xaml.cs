using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using DocumentFormat.OpenXml.Packaging;
using DOXW = DocumentFormat.OpenXml.Wordprocessing;

namespace Api
{
    public partial class MainWindow : Window
    {
        HttpClient h = new HttpClient();
        string f;

        public MainWindow() => InitializeComponent();

        async void GetData_Click(object s, RoutedEventArgs e)
        {
            try
            {
                var m = Regex.Match(await (await h.GetAsync("http://localhost:4444/TransferSimulator/fullName")).Content.ReadAsStringAsync(), "\"value\"\\s*:\\s*\"([^\"]+)\"");
                f = m.Success ? m.Groups[1].Value : "Ошибка парсинга";
                FioTextBox.Text = f;
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}\nЗапустите эмулятор"); }
        }

        void SendResult_Click(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(f)) { MessageBox.Show("Сначала нажмите 'Получить данные'"); return; }
            string r1 = !Regex.IsMatch(f, @"\d") ? "Пройдено" : "Не пройдено (есть цифры)";
            string r2 = Regex.IsMatch(f, @"^[a-zA-Zа-яА-ЯёЁ\s\-]+$") ? "Пройдено" : "Не пройдено (запрещённые символы)";
            ResultTextBlock.Text = $"Цифры: {r1}\nДопустимые символы: {r2}";
            string p = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ТестКейс.docx");
            if (!System.IO.File.Exists(p)) { MessageBox.Show("Файл ТестКейс.docx не найден"); return; }
            try
            {
                using (var w = WordprocessingDocument.Open(p, true)) { R(w, "Result1", r1); R(w, "Result2", r2); }
                MessageBox.Show("Записано в ТестКейс.docx");
            }
            catch (Exception ex) { MessageBox.Show("Ошибка записи: " + ex.Message); }
        }

        void R(WordprocessingDocument d, string n, string t)
        {
            var b = d.MainDocumentPart.Document.Body;
            foreach (var s in b.Descendants<DOXW.BookmarkStart>().Where(bk => bk.Name == n))
            {
                var e = b.Descendants<DOXW.BookmarkEnd>().FirstOrDefault(e2 => e2.Id == s.Id);
                var r = new DOXW.Run();
                r.AppendChild(new DOXW.Text(t));
                if (e != null)
                {
                    var toDel = s.Parent.Elements().SkipWhile(x => x != s).Skip(1).TakeWhile(x => x != e).ToList();
                    foreach (var x in toDel) x.Remove();
                }
                s.InsertAfterSelf(r);
                break;
            }
        }
    }
}