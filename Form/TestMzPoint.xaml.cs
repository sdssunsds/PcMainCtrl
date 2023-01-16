using GW.Function.ExcelFunction;
using Newtonsoft.Json;
using PcMainCtrl.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PcMainCtrl.Form
{
    public delegate void MzRun(Model.DataMzCameraLineModel model, ref int last_rgv_distance);
    /// <summary>
    /// TestMzPoint.xaml 的交互逻辑
    /// </summary>
    public partial class TestMzPoint : Window
    {
        public List<Model.Axis> AxesList { get; private set; }
        public List<Model.DataMzCameraLineModelEx> DataList
        {
            get
            {
                return dataGrid1.ItemsSource as List<Model.DataMzCameraLineModelEx>;
            }
            set
            {
                dataGrid1.ItemsSource = value;
            }
        }
        public MzRun MzRunAction;

        public TestMzPoint()
        {
            InitializeComponent();
            DataList = LocalDataBase.GetInstance().MzCameraDataListQurey();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "数据文件|*.xls;*.xlsx;*.json";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = "";
                textBox1.Text = openFileDialog.FileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "JSON文件|*.json";
                saveFileDialog.FileName = "DataMzCameraLineModel.json";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = saveFileDialog.FileName;
                    using (StreamWriter streamWriter = new StreamWriter(path))
                    {
                        streamWriter.Write(JsonConvert.SerializeObject(DataList)); 
                    }
                    path = new FileInfo(path).DirectoryName + "\\Axis.json";
                    using (StreamWriter streamWriter = new StreamWriter(path))
                    {
                        streamWriter.Write(JsonConvert.SerializeObject(AxesList));
                    }
                    MessageBox.Show("保存成功");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                int last = 0;
                foreach (var item in DataList)
                {
                    if (item.Enable)
                    {
                        MzRunAction(item, ref last);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int error_i = 0;
            try
            {
                if (string.IsNullOrEmpty(textBox1.Text))
                {
                    return;
                }
                if (!File.Exists(textBox1.Text))
                {
                    return;
                }

                error_i = 1;
                string extension = new FileInfo(textBox1.Text).Extension;
                if (extension.ToLower().Contains("json"))
                {
                    string json = "";
                    using (StreamReader sr = new StreamReader(textBox1.Text))
                    {
                        json = sr.ReadToEnd();
                    }
                    DataList = JsonConvert.DeserializeObject<List<Model.DataMzCameraLineModelEx>>(json);
                }
                else
                {
                    List<Model.DataMzCameraLineModelEx> list = new List<Model.DataMzCameraLineModelEx>();
                    ExcelModel excelModel = new ExcelModel(textBox1.Text);
                    List<int> distanceList = new List<int>();
                    Dictionary<int, List<Model.DataMzCameraLineModelEx>> front = new Dictionary<int, List<Model.DataMzCameraLineModelEx>>();
                    Dictionary<int, List<Model.DataMzCameraLineModelEx>> back = new Dictionary<int, List<Model.DataMzCameraLineModelEx>>();

                    error_i = 2;
                    for (int i = 1; i < excelModel[0].RowCount; i++)
                    {
                        error_i = 2000 + i;
                        error_i *= 10;
                        Model.DataMzCameraLineModelEx model = new Model.DataMzCameraLineModelEx();
                        model.DataLine_Index = int.Parse(excelModel[0][i][0].Value);
                        error_i++;
                        model.TrainModel = excelModel[0][i][1].Value;
                        error_i++;
                        model.TrainSn = excelModel[0][i][2].Value;
                        error_i++;
                        var lp = GetLocation(excelModel[0][i]);
                        model.Location = lp.location;
                        model.Point = lp.point;
                        error_i++;
                        model.Rgv_Distance = int.Parse(excelModel[0][i][9].Value);
                        if (!distanceList.Contains(model.Rgv_Distance))
                        {
                            distanceList.Add(model.Rgv_Distance);
                        }
                        error_i++;
                        model.sorIndex = int.Parse(excelModel[0][i][11].Value);
                        error_i++;
                        model.AxisID = int.Parse(excelModel[0][i][30].Value);
                        error_i++;
                        model.Axis_Distance = int.Parse(excelModel[0][i][31].Value);

                        string robot = excelModel[0][i][10].Value;
                        if (robot == "Front")
                        {
                            model.Front_Parts_Id = excelModel[0][i][5].Value;
                            model.FrontRobot_Id = int.Parse(excelModel[0][i][7].Value);
                            model.FrontComponentType = excelModel[0][i][8].Value;
                            model.FrontComponentId = excelModel[0][i][20].Value;
                            model.FrontRobot_J1 = excelModel[0][i][12].Value;
                            model.FrontRobot_J2 = excelModel[0][i][13].Value;
                            model.FrontRobot_J3 = excelModel[0][i][14].Value;
                            model.FrontRobot_J4 = excelModel[0][i][15].Value;
                            model.FrontRobot_J5 = excelModel[0][i][16].Value;
                            model.FrontRobot_J6 = excelModel[0][i][17].Value;
                            if (!string.IsNullOrEmpty(excelModel[0][i][19].Value))
                            {
                                model.Front3d_Id = int.Parse(excelModel[0][i][19].Value);
                            }

                            model.Back_Parts_Id = "0";
                            model.BackRobot_Id = 0;
                            model.BackComponentType = "0";
                            model.BackComponentId = "0";
                            model.BackRobot_J1 = "0.00";
                            model.BackRobot_J2 = "0.00";
                            model.BackRobot_J3 = "0.00";
                            model.BackRobot_J4 = "0.00";
                            model.BackRobot_J5 = "0.00";
                            model.BackRobot_J6 = "0.00";
                            model.Back3d_Id = 0;

                            if (!front.ContainsKey(model.Rgv_Distance))
                            {
                                front.Add(model.Rgv_Distance, new List<Model.DataMzCameraLineModelEx>());
                            }
                            front[model.Rgv_Distance].Add(model);
                        }
                        else
                        {
                            model.Back_Parts_Id = excelModel[0][i][5].Value;
                            model.BackRobot_Id = int.Parse(excelModel[0][i][7].Value);
                            model.BackComponentType = excelModel[0][i][8].Value;
                            model.BackComponentId = excelModel[0][i][20].Value;
                            model.BackRobot_J1 = excelModel[0][i][12].Value;
                            model.BackRobot_J2 = excelModel[0][i][13].Value;
                            model.BackRobot_J3 = excelModel[0][i][14].Value;
                            model.BackRobot_J4 = excelModel[0][i][15].Value;
                            model.BackRobot_J5 = excelModel[0][i][16].Value;
                            model.BackRobot_J6 = excelModel[0][i][17].Value;
                            if (!string.IsNullOrEmpty(excelModel[0][i][19].Value))
                            {
                                model.Back3d_Id = int.Parse(excelModel[0][i][19].Value);
                            }

                            model.Front_Parts_Id = "0";
                            model.FrontRobot_Id = 0;
                            model.FrontComponentType = "0";
                            model.FrontComponentId = "0";
                            model.FrontRobot_J1 = "0.00";
                            model.FrontRobot_J2 = "0.00";
                            model.FrontRobot_J3 = "0.00";
                            model.FrontRobot_J4 = "0.00";
                            model.FrontRobot_J5 = "0.00";
                            model.FrontRobot_J6 = "0.00";
                            model.Front3d_Id = 0;

                            if (!back.ContainsKey(model.Rgv_Distance))
                            {
                                back.Add(model.Rgv_Distance, new List<Model.DataMzCameraLineModelEx>());
                            }
                            back[model.Rgv_Distance].Add(model);
                        }
                    }

                    error_i = 3;
                    distanceList.Sort((x, y) => y - x);
                    foreach (int distance in distanceList)
                    {
                        List<Model.DataMzCameraLineModelEx> frontItem = null;
                        if (front.ContainsKey(distance))
                        {
                            frontItem = front[distance];
                            frontItem.Sort((x, y) => x.sorIndex - y.sorIndex);
                        }

                        List<Model.DataMzCameraLineModelEx> backItem = null;
                        if (back.ContainsKey(distance))
                        {
                            backItem = back[distance];
                            backItem.Sort((x, y) => x.sorIndex - y.sorIndex);
                        }

                        if (frontItem != null && backItem != null)
                        {
                            int length = backItem.Count > frontItem.Count ? backItem.Count : frontItem.Count;
                            for (int i = 0; i < length; i++)
                            {
                                if (i < frontItem.Count && i < backItem.Count)
                                {
                                    frontItem[i].Back_Parts_Id = backItem[i].Back_Parts_Id;
                                    frontItem[i].BackRobot_Id = backItem[i].BackRobot_Id;
                                    frontItem[i].BackComponentType = backItem[i].BackComponentType;
                                    frontItem[i].BackComponentId = backItem[i].BackComponentId;
                                    frontItem[i].Back3d_Id = backItem[i].Back3d_Id;
                                    frontItem[i].BackRobot_J1 = backItem[i].BackRobot_J1;
                                    frontItem[i].BackRobot_J2 = backItem[i].BackRobot_J2;
                                    frontItem[i].BackRobot_J3 = backItem[i].BackRobot_J3;
                                    frontItem[i].BackRobot_J4 = backItem[i].BackRobot_J4;
                                    frontItem[i].BackRobot_J5 = backItem[i].BackRobot_J5;
                                    frontItem[i].BackRobot_J6 = backItem[i].BackRobot_J6;
                                    list.Add(frontItem[i]);
                                }
                                else if (i < frontItem.Count)
                                {
                                    list.Add(frontItem[i]);
                                }
                                else if (i < backItem.Count)
                                {
                                    list.Add(backItem[i]);
                                }
                            }
                        }
                        else if (frontItem != null)
                        {
                            list.AddRange(frontItem);
                        }
                        else if (backItem != null)
                        {
                            list.AddRange(backItem);
                        }
                    }

                    DataList = list;

                    error_i = 4;
                    AxesList = new List<Model.Axis>();
                    for (int i = 1; i < excelModel[1].RowCount; i++)
                    {
                        error_i = 400 + i;
                        Model.Axis item = new Model.Axis();
                        item.ID = int.Parse(excelModel[1][i][0].Value);
                        item.Name = excelModel[1][i][1].Value;
                        item.Distance = int.Parse(excelModel[1][i][2].Value);
                        AxesList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("["+ error_i + "]" + ex.Message);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in DataList)
            {
                item.Enable = checkBox1.IsChecked.Value;
            }
            var data = dataGrid1.ItemsSource;
            dataGrid1.ItemsSource = null;
            dataGrid1.ItemsSource = data;
        }

        private (string location, string point) GetLocation(ExcelRow excelRow)
        {
            string location = "{0}_{1}";
            int index = 0;
            if (int.TryParse(excelRow[3].Value, out _))
            {
                index = int.Parse(excelRow[3].Value) - 1;
            }
            int index2 = 0;
            string poi = "";
            switch (excelRow[6].Value)
            {
                case "011":
                    index2 = 0;
                    poi = "z1";
                    break;
                case "012":
                    index2 = 0;
                    poi = "z2";
                    break;
                case "021":
                    index2 = 1;
                    poi = "z1";
                    break;
                case "022":
                    index2 = 1;
                    poi = "z2";
                    break;
                default:
                    index2 = 0;
                    poi = "";
                    break;
            }
            return (string.Format(location, index, index2), poi);
        }
    }

    public static class DragDropRowBehavior
    {
        private static DataGrid dataGrid;

        private static Popup popup;

        private static bool enable;

        private static object draggedItem;

        public static object DraggedItem
        {
            get { return DragDropRowBehavior.draggedItem; }
            set { DragDropRowBehavior.draggedItem = value; }
        }

        public static Popup GetPopupControl(DependencyObject obj)
        {
            return (Popup)obj.GetValue(PopupControlProperty);
        }

        public static void SetPopupControl(DependencyObject obj, Popup value)
        {
            obj.SetValue(PopupControlProperty, value);
        }

        public static readonly DependencyProperty PopupControlProperty =
            DependencyProperty.RegisterAttached("PopupControl", typeof(Popup), typeof(DragDropRowBehavior), new UIPropertyMetadata(null, OnPopupControlChanged));

        private static void OnPopupControlChanged(DependencyObject depObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || !(e.NewValue is Popup))
            {
                throw new ArgumentException("Popup Control should be set", "PopupControl");
            }
            popup = e.NewValue as Popup;
            dataGrid = depObject as DataGrid;
            if (dataGrid == null)
                return;

            if (enable && popup != null)
            {
                dataGrid.BeginningEdit += new EventHandler<DataGridBeginningEditEventArgs>(OnBeginEdit);
                dataGrid.CellEditEnding += new EventHandler<DataGridCellEditEndingEventArgs>(OnEndEdit);
                dataGrid.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(OnMouseLeftButtonUp);
                dataGrid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
                dataGrid.MouseMove += new MouseEventHandler(OnMouseMove);
            }
            else
            {
                dataGrid.BeginningEdit -= new EventHandler<DataGridBeginningEditEventArgs>(OnBeginEdit);
                dataGrid.CellEditEnding -= new EventHandler<DataGridCellEditEndingEventArgs>(OnEndEdit);
                dataGrid.MouseLeftButtonUp -= new System.Windows.Input.MouseButtonEventHandler(OnMouseLeftButtonUp);
                dataGrid.MouseLeftButtonDown -= new MouseButtonEventHandler(OnMouseLeftButtonDown);
                dataGrid.MouseMove -= new MouseEventHandler(OnMouseMove);

                dataGrid = null;
                popup = null;
                draggedItem = null;
                IsEditing = false;
                IsDragging = false;
            }
        }

        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(DragDropRowBehavior), new UIPropertyMetadata(false, OnEnabledChanged));

        private static void OnEnabledChanged(DependencyObject depObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool == false)
                throw new ArgumentException("Value should be of bool type", "Enabled");
            enable = (bool)e.NewValue;
        }

        public static bool IsEditing { get; set; }

        public static bool IsDragging { get; set; }

        private static void OnBeginEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            IsEditing = true;
            if (IsDragging) ResetDragDrop();
        }

        private static void OnEndEdit(object sender, DataGridCellEditEndingEventArgs e)
        {
            IsEditing = false;
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEditing) return;

            var row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(dataGrid));
            if (row == null || row.IsEditing) return;

            IsDragging = true;
            DraggedItem = row.Item;
        }

        private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsDragging || IsEditing)
            {
                return;
            }
            dataGrid.Cursor = Cursors.Arrow;
            IList list = dataGrid.ItemsSource as IList;
            var targetItem = dataGrid.SelectedItem;
            if (targetItem == null || !ReferenceEquals(DraggedItem, targetItem))
            {
                dataGrid.ItemsSource = null;
                var targetIndex = list.IndexOf(targetItem);
                list.Remove(DraggedItem);
                list.Insert(targetIndex, DraggedItem);
                dataGrid.ItemsSource = list;
                dataGrid.SelectedItem = DraggedItem;
            }
            ResetDragDrop();
        }

        private static void ResetDragDrop()
        {
            IsDragging = false;
            popup.IsOpen = false;
            dataGrid.IsReadOnly = false;
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging || e.LeftButton != MouseButtonState.Pressed) return;
            if (dataGrid.Cursor != Cursors.SizeAll) dataGrid.Cursor = Cursors.SizeAll;
            popup.DataContext = DraggedItem;
            if (!popup.IsOpen)
            {
                dataGrid.IsReadOnly = true;
                popup.IsOpen = true;
            }

            Size popupSize = new Size(popup.ActualWidth, popup.ActualHeight);
            popup.PlacementRectangle = new Rect(e.GetPosition(dataGrid), popupSize);
            Point position = e.GetPosition(dataGrid);
            var row = UIHelpers.TryFindFromPoint<DataGridRow>(dataGrid, position);
            if (row != null) dataGrid.SelectedItem = row.Item;
        }
    }

    public static class UIHelpers
    {
        public static T TryFindParent<T>(DependencyObject child)
          where T : DependencyObject
        {
            DependencyObject parentObject = GetParentObject(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return TryFindParent<T>(parentObject);
            }
        }

        public static DependencyObject GetParentObject(DependencyObject child)
        {
            if (child == null) return null;
            ContentElement contentElement = child as ContentElement;

            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            return VisualTreeHelper.GetParent(child);
        }

        public static void UpdateBindingSources(DependencyObject obj,
                                  params DependencyProperty[] properties)
        {
            foreach (DependencyProperty depProperty in properties)
            {
                BindingExpression be = BindingOperations.GetBindingExpression(obj, depProperty);
                if (be != null) be.UpdateSource();
            }

            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                DependencyObject childObject = VisualTreeHelper.GetChild(obj, i);
                UpdateBindingSources(childObject, properties);
            }
        }

        public static T TryFindFromPoint<T>(UIElement reference, Point point)
          where T : DependencyObject
        {
            DependencyObject element = reference.InputHitTest(point)
                                         as DependencyObject;
            if (element == null) return null;
            else if (element is T) return (T)element;
            else return TryFindParent<T>(element);
        }
    }
}
