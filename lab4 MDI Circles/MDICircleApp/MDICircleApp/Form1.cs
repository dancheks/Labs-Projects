using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MDICircleApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            IsMdiContainer = true;
            Text = "MDI Circle Drawing App";

            // Меню для добавления новых дочерних окон
            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem newItem = new ToolStripMenuItem("New");
            newItem.Click += (sender, args) => OpenNewChildForm();
            fileMenu.DropDownItems.Add(newItem);
            menuStrip.Items.Add(fileMenu);
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);
        }
        private void OpenNewChildForm()
        {
            // Создание нового дочернего окна
            ChildForm childForm = new ChildForm();
            childForm.MdiParent = this;
            childForm.Show();
        }

    }
    public class ChildForm : Form
    {
        private List<Circle> circles = new List<Circle>();

        public ChildForm()
        {
            Text = "Child Window";
            DoubleBuffered = true; // Для устранения мерцания при рисовании
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Определяем тип круга (закрашенный или сплошной)
            if (circles.Count < 5)
            {
                Circle circle = new Circle
                {
                    Position = e.Location,
                    Filled = e.Button == MouseButtons.Left, // ЛКМ - закрашенный, ПКМ - контур
                    Radius = 30
                };
                circles.Add(circle);
                Invalidate(); // Перерисовываем форму
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            // Рисуем все круги
            foreach (var circle in circles)
            {
                if (circle.Filled)
                {
                    g.FillEllipse(Brushes.Blue, circle.Position.X - circle.Radius, circle.Position.Y - circle.Radius, circle.Radius * 2, circle.Radius * 2);
                }
                else
                {
                    g.DrawEllipse(Pens.Blue, circle.Position.X - circle.Radius, circle.Position.Y - circle.Radius, circle.Radius * 2, circle.Radius * 2);
                }
            }
        }
    }
    public class Circle
    {
        public Point Position { get; set; }
        public bool Filled { get; set; }
        public int Radius { get; set; }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
