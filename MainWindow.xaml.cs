using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace TemplateReader
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Type> typeList;
        public MainWindow()
        {
            InitializeComponent();
            typeList = new List<Type>();
            LoadControls();
        }

        private void LoadControls()
        {
            var controlType = typeof(Control);
            var assembly = controlType.Assembly;
            var types = assembly.GetTypes();

            foreach(var t in types)
            {
                if(t.IsSubclassOf(controlType) && t.IsPublic && !t.IsAbstract)
                {
                    typeList.Add(t);
                }
            }
            lbControls.ItemsSource = typeList;
        }

        private void lbControls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = lbControls.SelectedIndex;
            if (index >= 0)
            {
                tbXaml.Text = GetTemplateXaml(typeList[index]);
                this.Title = "Current Control:"+typeList[index].Name;
            }
        }

        private string GetTemplateXaml(Type t)
        {
            //构造一个对象
            ConstructorInfo info = t.GetConstructor(System.Type.EmptyTypes);
            if (info == null)
            {
                return "";
            }
            Control control = (Control)info.Invoke(null);
            //将对象加入到当前进程中，才能获取对应的Template
            Window win = control as Window;
            if (win != null)
            {
                // Create the window (but keep it minimized).
                win.WindowState = System.Windows.WindowState.Minimized;
                win.ShowInTaskbar = false;
                win.Show();
            }
            else
            {
                // Add it to the grid (but keep it hidden).
                control.Visibility = Visibility.Collapsed;
                grid.Children.Add(control);
            }

            var template=control.Template;
            if (win != null)
            {
                win.Close();
            }
            else
            {
                grid.Children.Remove(control);
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, settings);
            XamlWriter.Save(template, writer);

            return sb.ToString();

        }
    }
}
