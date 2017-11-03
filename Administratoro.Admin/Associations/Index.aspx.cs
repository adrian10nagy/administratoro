
namespace Admin.Associations
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public partial class Index : System.Web.UI.Page
    {

        protected void Page_Init(object sender, EventArgs e)
        {
            var estate = Session[SessionConstants.SelectedEstate];
            if (estate != null)
            {
                var es = (Estates)estate;
                estateName.InnerText = es.Name;
                InitializeStairs(es);
                estateIndiviza.Text = es.Indiviza.ToString();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private void InitializeStairs(Estates estate)
        {
            estateStairsAdded.Controls.Clear();
            
            if (estate.HasStaircase)
            {
                estateStairsAdded.Visible = true;
                btnEstateStairsNew.Visible = true;
                estateStairs.SelectedIndex = 1;
                foreach (var item in estate.StairCases)
                {
                    TextBox tb = new TextBox
                    {
                        Text = item.Value,
                        Enabled = false
                    };
                    estateStairsAdded.Controls.Add(tb);

                    LinkButton stairsChange = new LinkButton
                    {
                        Text = "Modifică",
                        CommandName = item.Id.ToString(),
                    };
                    stairsChange.Click += stairsChange_Click;
                    estateStairsAdded.Controls.Add(stairsChange);

                    LinkButton stairsRemove = new LinkButton
                    {
                        Text = "Sterge",
                        CommandName = item.Id.ToString(),
                    };
                    stairsRemove.Click += stairsRemove_Click;
                    estateStairsAdded.Controls.Add(stairsRemove);
                    estateStairsAdded.Controls.Add(new LiteralControl("<br />"));
                }
            }
            else
            {
                estateStairsAdded.Visible = false;
                estateStairs.SelectedIndex = 0;
                btnEstateStairsNew.Visible = false;
            }
        }

        private void stairsRemove_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;

             var estate = Session[SessionConstants.SelectedEstate];

            if (estate != null)
            {
                var es = (Estates)estate;
                int stairCaseId;
                if (int.TryParse(btn.CommandName,out stairCaseId))
                {
                    StairCasesManager.Remove(stairCaseId, es.Id);
                    InitializeStairs(es);
                }
            }
        }

        private void stairsChange_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected void estateStairs_SelectedIndexChanged(object sender, EventArgs e)
        {
            var estate = Session[SessionConstants.SelectedEstate];

            if (estate != null)
            {
                var es = (Estates)estate;

                EstatesManager.UpdateStairs(es, estateStairs.SelectedIndex == 1);
                if (estateStairs.SelectedIndex == 0)
                {
                    estateStairsAdded.Visible = false;
                    btnEstateStairsNew.Visible = false;
                    txtEstateStairsNew.Visible = false;
                }
                else
                {
                    InitializeStairs(es);
                    txtEstateStairsNew.Visible = false;
                    txtEstateStairsNew.Enabled = false;
                }

            }
        }

        protected void btnEstateStairsNew_Click(object sender, EventArgs e)
        {
            //add
            var estate = Session[SessionConstants.SelectedEstate];
            if (estate != null)
            {
                var es = (Estates)estate;

                if (txtEstateStairsNew.Visible)
                {
                    if (StairCaseAddNew(es))
                    {
                        txtEstateStairsNew.Visible = false;
                        txtEstateStairsNew.Text = string.Empty;
                    }
                    else
                    {
                        InitializeStairs(es);
                    }
                }
                else
                {
                    txtEstateStairsNew.Visible = true;
                    txtEstateStairsNew.Enabled = true;
                }
            }
        }

        private bool StairCaseAddNew(Estates estate)
        {
            txtEstateStairsNew.Attributes.CssStyle.Add("border-color", "");

            if (string.IsNullOrEmpty(txtEstateStairsNew.Text) || estate == null)
            {
                txtEstateStairsNew.Attributes.CssStyle.Add("border-color", "red");
                return false;
            }

            StairCasesManager.AddNew(txtEstateStairsNew.Text, estate);
            var newEstate = EstatesManager.GetById(estate.Id);
            Session[SessionConstants.SelectedEstate] = newEstate;
            InitializeStairs(newEstate);

            return true;
        }
    }
}