using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO.Compression;
using ED = SRB2_Emblem_Manager.EmblemDrawer;
using ED_Res = SRB2_Emblem_Manager.EmblemDrawerRessources;

namespace SRB2_Emblem_Manager
{
    public partial class MainForm : Form
    {
        private static Memory m;
        public MainForm()
        {
            InitializeComponent();

            m = new Memory();
            m.EmblemsChangedEvent += EmblemsChanged;
            /*ZipArchive idk = ZipFile.OpenRead("zones.pk3");
            foreach (ZipArchiveEntry entry in idk.Entries)
            {
                Console.WriteLine(entry.Name + "    " + entry.FullName);
            }
            System.Threading.Thread.Sleep(15000);*/

            ED.ReadPalette(ED_Res.PLAYPAL);
            listBox1.SelectedIndexChanged += (o,e) => Invalidate();
        }

        public byte[] GetLumpBytes(char sprite)
        {
            switch (sprite)
            {
                case 'A': return ED_Res.EMBMA0;
                case 'B': return ED_Res.EMBMB0;
                case 'C': return ED_Res.EMBMC0;
                case 'D': return ED_Res.EMBMD0;
                case 'E': return ED_Res.EMBME0;
                case 'F': return ED_Res.EMBMF0;
                case 'G': return ED_Res.EMBMG0;
                case 'H': return ED_Res.EMBMH0;
                case 'I': return ED_Res.EMBMI0;
                case 'J': return ED_Res.EMBMJ0;
                case 'K': return ED_Res.EMBMK0;
                case 'L': return ED_Res.EMBML0;
                case 'M': return ED_Res.EMBMM0;
                case 'N': return ED_Res.EMBMN0;
                case 'O': return ED_Res.EMBMO0;
                case 'P': return ED_Res.EMBMP0;
                case 'Q': return ED_Res.EMBMQ0;
                case 'R': return ED_Res.EMBMR0;
                case 'S': return ED_Res.EMBMS0;
                case 'T': return ED_Res.EMBMT0;
                case 'U': return ED_Res.EMBMU0;
                case 'V': return ED_Res.EMBMV0;
                case 'W': return ED_Res.EMBMW0;
                case 'X': return ED_Res.EMBMX0;
                case 'Y': return ED_Res.EMBMY0;
                case 'Z': return ED_Res.EMBMZ0;

                default: return ED_Res.EMBMN0;
            }
        }

        private void EmblemsChanged(object sender, EmblemsChangedEventArgs e)
        {
            Invoke(new MethodInvoker(() => 
            {
                Console.WriteLine(sender + "   " + e.IsFullInfo);
                if (e.IsFullInfo)
                {
                    listBox1.BeginUpdate();

                    listBox1.Items.Clear();
                    int alreadyAddedLevel = -1;
                    foreach (Emblem emb in m.Emblems.OrderBy(emblem => emblem.level))
                    {
                        if (alreadyAddedLevel != emb.level)
                        {
                            listBox1.Items.Add(emb.level);
                            alreadyAddedLevel = emb.level;
                        }
                    }

                    listBox1.EndUpdate();
                }
                Invalidate();
            }));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                IEnumerable<Emblem> levelemblems = m.Emblems.Where(emblem => emblem.level == int.Parse(listBox1.SelectedItem.ToString()));
                Point embLoc = new Point(500, 100);
                foreach(Emblem emb in levelemblems)
                {
                    e.Graphics.DrawImage(ED.ColoredBitmapFromFile(GetLumpBytes(emb.sprite), emb.color), embLoc);
                    embLoc.Y += 40;
                }
            }
            base.OnPaint(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                m.ReadEmblemFullInfo();
                Console.WriteLine(m.GetEmblemCount());
            }).Start();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}