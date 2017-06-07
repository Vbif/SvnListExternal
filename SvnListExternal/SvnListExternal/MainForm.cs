using SharpSvn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace SvnListExternal
{
    public partial class MainForm : Form
    {
        public class ExternalItem
        {
            public String Path;
            public String PathToExternal;

            public override string ToString()
            {
                return Path;
            }
        }

        public String TargetPath {get; set;}

        private List<ExternalItem> data = new List<ExternalItem>();
        private BackgroundWorker worker;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (TargetPath == null)
            {
                listBox.Items.Add("Drag and drop folder here");
                return;
            }

            Start();
        }

        private void Start()
        {
            listBox.DataSource = null;
            data.Clear();

            listBox.Items.Clear();
            listBox.Items.Add("Loading...");

            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            worker.RunWorkerAsync();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            listBox.DataSource = data;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            using (var wcClient = new SvnWorkingCopyClient())
            using (var svnClient = new SvnClient())
            {
                Walk(wcClient, svnClient, TargetPath);
            }
        }

        private void Walk(SvnWorkingCopyClient wcClient, SvnClient svnClient, String uri)
        {
            Collection<SvnWorkingCopyEntryEventArgs> list;
            wcClient.GetEntries(uri, out list);

            foreach (var i in list)
            {
                if (i.FullPath == uri)
                    continue;

                if (i.NodeKind == SvnNodeKind.Directory)
                {
                    Collection<SvnPropertyListEventArgs> listProp;
                    bool success = svnClient.GetPropertyList(SvnTarget.FromString(i.FullPath), out listProp);
                    Debug.Assert(success);

                    foreach (var p in listProp)
                    {
                        foreach (var p2 in p.Properties)
                        {
                            if (p2.Key.Contains("ext"))
                            {
                                data.Add(new ExternalItem()
                                {
                                    Path = i.FullPath,
                                    PathToExternal = p2.Key
                                });
                            }
                        }
                    }
                }

                if (i.NodeKind == SvnNodeKind.Directory)
                    Walk(wcClient, svnClient, i.FullPath);
            }
        }

        private void listBox_Click(object sender, EventArgs e)
        {
            var path = listBox.SelectedItem as ExternalItem;
            if (path == null)
                return;

            Process.Start(Path.GetDirectoryName(path.Path));
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var path = e.Data.GetData("FileName") as String[];
            if (path == null)
                return;

            if (worker != null && worker.IsBusy)
                return;

            TargetPath = path[0];
            Start();
        }
    }
}
