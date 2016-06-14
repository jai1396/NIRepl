﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NIReplCS
{
    class MainViewModel : DependencyObject
    {
        #region DispText
        public string DispText
        {
            get { return (string)GetValue(DispTextProperty); }
            set { SetValue(DispTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DispText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DispTextProperty =
            DependencyProperty.Register("DispText", typeof(string), typeof(MainViewModel));
        #endregion

        public ObservableCollection<OutputLine> ConsoleHistory { get; set; }

        public int CaretPosition { get; set; }

        OutputModule terminal;

        private ICommand enterKeyCommand;
        public ICommand EnterKeyCommand {
            get
            {
                return enterKeyCommand
                    ?? (enterKeyCommand = new EnterActionCommand(() =>
                    {
                        //MessageBox.Show(DispText);

                        //DispText = NewDispText(DispText);

                        ConsoleHistory.Add(new OutputLine(DispText));

                        terminal.RunCommand(DispText);

                        string output = terminal.GetLastOutput();

                        if (output!=null && output!="")
                        {
                            ConsoleHistory.Add(new OutputLine(output));
                        }

                        
                        DispText = "";




                    }));
            }
        }

        private string NewDispText(string dispText)
        {
            return string.Concat(dispText, "\n> ");
        }

        public MainViewModel()
        {
            DispText = "";
            ConsoleHistory = new ObservableCollection<OutputLine>();

            terminal = new OutputModule();

        }
    }
}
