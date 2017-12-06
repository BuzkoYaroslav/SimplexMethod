using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using library;
using System.IO;

namespace SimplexMethod
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


        }

        private int varsCount = 0;
        private int condsCount = 0;
        private List<int> basisIndexes;

        private void UpdateVarsCount(int difference)
        {
            if (difference == 0)
                return;

            if (dataGridView1.Rows.Count == 0)
                dataGridView1.Rows.Add(new DataGridViewRow());

            for (int i = 0; i < Math.Abs(difference); i++)
                if (difference > 0)
                {
                    dataGridView1.Columns.Add(new DataGridViewColumn());
                    DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                    dataGridView1.Rows[0].Cells.Add(cell);
                } else
                {
                    dataGridView1.Columns.RemoveAt(dataGridView1.Columns.Count - 1);
                }

            if (condsCount == 0)
                return;

            for (int i = 0; i < Math.Abs(difference); i++)
                if (difference > 0)
                {
                    dataGridView2.Columns.Add(new DataGridViewColumn());

                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                        row.Cells.Add(cell);
                    }
                }
                else
                {
                    dataGridView2.Columns.RemoveAt(dataGridView2.Columns.Count - 1);
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            varsCount = Convert.ToInt32(numericUpDown1.Value);
            condsCount = Convert.ToInt32(numericUpDown2.Value);

            listBox1.Items.Clear();
            numericUpDown3.Maximum = varsCount - 1;
            if (basisIndexes != null)
                basisIndexes.Clear();
            else
                basisIndexes = new List<int>();

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView2.Rows.Clear();
            dataGridView2.Columns.Clear();

            for (int i = 0; i < varsCount; i++)
                dataGridView1.Columns.Add(GetNewColumn());

            DataGridViewRow row1 = new DataGridViewRow();
            
            for (int i = 0; i < varsCount; i++)
                row1.Cells.Add(new DataGridViewTextBoxCell());
            dataGridView1.Rows.Add(row1);

            for (int i = 0; i <= varsCount; i++)
                dataGridView2.Columns.Add(GetNewColumn());

            for (int i = 0; i < condsCount; i++)
            {
                DataGridViewRow row2 = new DataGridViewRow();

                for (int j = 0; j <= varsCount; j++)
                    row2.Cells.Add(new DataGridViewTextBoxCell());

                dataGridView2.Rows.Add(row2);
            }
        }

        private DataGridViewColumn GetNewColumn()
        {
            DataGridViewColumn column = new DataGridViewColumn();

            column.CellTemplate = new DataGridViewTextBoxCell();

            return column;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double[] coefs = new double[varsCount];
            double[,] conditionsMatrix = new double[condsCount, varsCount + 1];

            for (int i = 0; i < varsCount; i++)
                coefs[i] = Convert.ToDouble(dataGridView1.Rows[0].Cells[i].Value);
            for (int i = 0; i < condsCount; i++)
                for (int j = 0; j <= varsCount; j++)
                    conditionsMatrix[i, j] = Convert.ToDouble(dataGridView2.Rows[i].Cells[j].Value);

            SimplexMethod method = new SimplexMethod(conditionsMatrix, coefs, basisIndexes.ToArray(), comboBox1.SelectedIndex == 0);

            method.Solve("result.txt");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (basisIndexes == null)
                return;

            if (basisIndexes.Count < varsCount &&
                !basisIndexes.Contains(Convert.ToInt32(numericUpDown3.Value)))
            {
                basisIndexes.Add(Convert.ToInt32(numericUpDown3.Value));
                listBox1.Items.Add(basisIndexes[basisIndexes.Count - 1]);
            }
                
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;

            if (index == -1)
                return;

            basisIndexes.RemoveAt(index);
            listBox1.Items.RemoveAt(index);
        }
    }
}
