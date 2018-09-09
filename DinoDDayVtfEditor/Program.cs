using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DinoDDayVtfEditor
{
    static class Program
    {
        public const string VTF_FILE_EXTENSION = ".vtf";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string us = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).TrimEnd('\\') + @"\\rpvtf\\";

            //First, check if the application was registered.
            if (Registry.CurrentUser.OpenSubKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + VTF_FILE_EXTENSION) == null && !File.Exists(us+"flag_to_not_ask"))
            {
                //Not registered. Register this now.
                var msg = MessageBox.Show("Administrator is needed to associate "+VTF_FILE_EXTENSION+" with this viewer. This'll allow you to double click on these files to view them. Is that okay?", VTF_FILE_EXTENSION + " isn't registered.", MessageBoxButtons.YesNo);
                if(msg == DialogResult.Yes)
                {
                    //Register.
                    Process p = Process.Start(us+@"rpvtf\\SetupVtfFile.exe");
                    p.WaitForExit();
                    MessageBox.Show(VTF_FILE_EXTENSION + " was added. You can now double click a VTF file to view it. Thank you!", "Registered");
                } else
                {
                    MessageBox.Show("You won't be asked again. Thank you!", "Not registered.");
                    File.WriteAllText(us + "flag_to_not_ask","hi");
                }
                return;
            }

            //If the last file still exists, delete it.
            if(File.Exists(us + "last_file_cleanup"))
            {
                string lastFile = File.ReadAllText(us + "last_file_cleanup");
                if(File.Exists(lastFile))
                {
                    try
                    {
                        File.Delete(lastFile);
                    } catch
                    {

                    }
                }
            }

            //Do the conversion.
            string[] openedPaths = Environment.GetCommandLineArgs();
            if(openedPaths[0] == Application.ExecutablePath && openedPaths.Length <= 1)
            {
                MessageBox.Show("No VTF files were requested. Run this from the \"open with\" menu in Windows after right clicking a file.", "Oops");
            } else
            {
                //Open each.
                foreach(string path in openedPaths)
                {
                    if(path != Application.ExecutablePath)
                    {
                        try
                        {
                            //First, edit it so it'll open in VTFcmd
                            Random r = new Random();
                            string name = DateTime.UtcNow.Ticks.ToString() + r.Next(0, int.MaxValue).ToString();
                            Directory.CreateDirectory("temp");
                            string tmp = us + "temp\\" + name + "\\";
                            Directory.CreateDirectory(tmp);
                            //Read in the VTF file.
                            byte[] data = File.ReadAllBytes(path);
                            //Edit the byte.
                            data[8] = 0x04;
                            //Save
                            File.WriteAllBytes(tmp + "input.vtf", data);
                            //Convert
                            ProcessStartInfo i = new ProcessStartInfo(us + "VtfEdit\\VTFCmd.exe", @"-file " + tmp + @"input.vtf -exportformat png");
                            i.WorkingDirectory = Application.StartupPath;
                            i.WindowStyle = ProcessWindowStyle.Hidden;
                            Process p = Process.Start(i);
                            p.WaitForExit();
                            //Check if the file exists where it should.
                            string output = tmp + @"input.png";
                            try
                            {
                                var viewer = Process.Start(output);
                            } catch
                            {

                            }
                            //Clean up
                            File.Delete(tmp + "input.vtf");
                            //Write the last directory used. This'll be removed next time the program starts.
                            File.WriteAllText(us + "last_file_cleanup", output);
                        } catch (Exception ex)
                        {
                            MessageBox.Show("Failed to convert " + path + "."+ "\r\n\r\nError: " + ex.Message + "\r\n\r\nStack: " + ex.StackTrace, "A VTF file failed.");
                        }
                    }
                }
            }

            


        }


        
    }
}
