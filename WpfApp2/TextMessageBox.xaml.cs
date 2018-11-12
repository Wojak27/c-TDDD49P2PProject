﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TextMessageBox : UserControl, INotifyPropertyChanged
    {
        public TextMessageBox(string fromUser, string messageText)
        {
            InitializeComponent();
            user.Text = fromUser;
            textBox.Text = messageText;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void setText(String text)
        {
            textBox.Text = text;
        }

        public String getText()
        {
            return textBox.Text;
        }

        public void setImage(Image image)
        {
            imageBox = image;
        }

        public Image getImage()
        {
            return imageBox;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
