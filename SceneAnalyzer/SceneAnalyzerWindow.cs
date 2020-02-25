/*
 此Mod依赖于UnityWinforms仓库
 */
using Harmony12;
using System.Drawing;
using UE = UnityEngine;
using System.Windows.Forms;
using UnityWinForms.Examples;
using UnityWinForms.Examples.Panels;

namespace SceneAnalyzer
{
    /// <summary>
    /// 场景分析窗口
    /// </summary>
    public class SceneAnalyzerWindow : Form
    {
        private readonly TreeView treeView;
        private BaseExamplePanel currentPanel;

        public SceneAnalyzerWindow()
        {
            Text = "场景分析器";
            Size = new Size(800, 600);
            
            //场景树
            treeView = new TreeView();
            treeView.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            treeView.Location = new Point(0, uwfHeaderHeight - 1);
            treeView.Height = Height - uwfHeaderHeight + 1;
            treeView.TabStop = false;
            treeView.Width = 220;
            treeView.NodeMouseClick += OnNodeClick;
            Controls.Add(treeView);
            RefreshTreeView();
        }

        /// <summary>
        /// 刷新树
        /// </summary>
        public void RefreshTreeView()
        {
            treeView.Nodes.Clear();
            TreeNode scene = new TreeNode("场景: " + UE.SceneManagement.SceneManager.GetActiveScene().name);

            var rootobjs = UE.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            
            foreach(var obj in rootobjs)
            {
                scene.Nodes.Add(GetChildNode(obj.transform));
            }

            treeView.Nodes.Add(scene);
            treeView.Refresh();
        }

        private TreeNode GetChildNode(UE.Transform transform)
        {
            TreeNode node = new TreeNode(transform.gameObject.name);
            node.Tag = transform;
            if (transform.childCount == 0)
            {
                return node;
            }
            else
            {
                for(int i = 0; i < transform.childCount; i++)
                {
                    node.Nodes.Add(GetChildNode(transform.GetChild(i)));
                }
                return node;
            }
        }

        public void OnNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.Node == null || e.Node.Tag == null)
                return;

            SetPanel(new DetailPanel(e.Node.Tag as UE.Transform));
        }

        public void SetPanel(BaseExamplePanel panel)
        {
            if (currentPanel != null && !currentPanel.IsDisposed)
                currentPanel.Dispose();

            currentPanel = panel;
            currentPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            currentPanel.Location = new Point(treeView.Location.X + treeView.Width, uwfHeaderHeight);
            currentPanel.Height = Height - uwfHeaderHeight;
            currentPanel.Width = Width - treeView.Width;
            Controls.Add(currentPanel);
            currentPanel.Initialize();
        }
    }

    /// <summary>
    /// 细节面板
    /// </summary>
    public class DetailPanel : BaseExamplePanel
    {
        private UE.Transform transform;
        public DetailPanel(UE.Transform transform)
        {
            this.transform = transform;
        }

        public override void Initialize()
        {
            var coms = transform.GetComponents<UE.Component>();
            this.Create<Label>(transform.name + "组件数:" + coms.Length);
            var tree = this.Create<TreeView>();
            tree.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            tree.Width = this.Width - 1;
            tree.Height = Height - 31;
            tree.Location = new Point(0, 30);
            tree.TabStop = false;
            foreach (var com in coms)
            {
                var comnode = new TreeNode(com.GetType().ToString());
                var comt = Traverse.Create(com);
                var pnode = new TreeNode("属性");
                foreach(var p in comt.Properties())
                {
                    object v = comt.Property(p).GetValue();
                    if(v == null)
                    {
                        pnode.Nodes.Add(new TreeNode($"{p} : null"));
                    }
                    else
                    {
                        pnode.Nodes.Add(new TreeNode($"{p} : {v.ToString()}"));
                    }
                }
                var fnode = new TreeNode("字段");
                foreach (var f in comt.Fields())
                {
                    object v = comt.Field(f).GetValue();
                    if (v == null)
                    {
                        pnode.Nodes.Add(new TreeNode($"{f} : null"));
                    }
                    else
                    {
                        pnode.Nodes.Add(new TreeNode($"{f} : {v.ToString()}"));
                    }
                }
                comnode.Nodes.Add(pnode);
                comnode.Nodes.Add(fnode);
                tree.Nodes.Add(comnode);
            }
            tree.ExpandAll();
            tree.Refresh();
        }
    }
}
