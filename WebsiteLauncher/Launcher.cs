﻿/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /Launcher.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * Responsible for launching the web and/or database server(s); this is also used to
 * display a notification icon to the user with a context-menu.
 */
using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;

namespace WebsiteLauncher
{
    public partial class Launcher : Form
    {
        #region "Variables"
        private Settings settingsWindow = null;
        private Process db = null;
        private Process webserver = null;
        private int webPort;
        #endregion

        #region "Methods - Constructors"
        public Launcher()
        {
            InitializeComponent();
        }
        #endregion

        #region "Methods - Events"
        private void Launcher_Load(object sender, EventArgs e)
        {
            // Hide the window
            Hide();
            // Load the config
            XmlDocument doc;
            try
            {
                string config = File.ReadAllText(Program.PATH_CONFIG);
                doc = new XmlDocument();
                doc.LoadXml(config);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load configuration! If this persists, delete the file 'Launcher.xml' in the same directory as this application (" + Application.StartupPath + ")...\r\n\r\nError:\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.StackTrace, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return;
            }
            bool hideWindow;
            try
            {
                hideWindow = doc["settings"]["hide_windows"].InnerText == "1";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load window-hide configuration! If this persists, delete the file 'Launcher.xml' in the same directory as this application (" + Application.StartupPath + ")...\r\n\r\nError:\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.StackTrace, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return;
            }
            // Launch the database-server
            try
            {
                if (doc["settings"]["launch"]["database"].InnerText == "1")
                {
                    db = new Process();
                    db.StartInfo.WorkingDirectory = Program.PATH_DB + "\\bin";
                    db.StartInfo.FileName = "mysqld.exe";
                    db.StartInfo.WindowStyle = hideWindow ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
                    db.Start();
                    Thread.Sleep(1000); // To allow the process to start
                    if (db.HasExited) throw new Exception("Database-server process has unexpectedly exited!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to launch database-server! If this persists, delete the file 'Launcher.xml' in the same directory as this application (" + Application.StartupPath + ")...\r\n\r\nError:\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.StackTrace, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return;
            }
            // Launch the web-server
            try
            {
                if (doc["settings"]["launch"]["web"].InnerText == "1")
                {
                    webserver = new Process();
                    webserver.StartInfo.WorkingDirectory = Program.PATH_WEB;
                    webserver.StartInfo.FileName = "UltiDevCassinWebServer2a.exe";
                    webPort = int.Parse(doc["settings"]["web_port"].InnerText);
                    webserver.StartInfo.Arguments = "/run \"" + Program.PATH_WEBSITE + "\" \"Default.aspx\" \"" + webPort + "\" nobrowser";
                    webserver.StartInfo.WindowStyle = hideWindow ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
                    webserver.Start();
                    Thread.Sleep(1000); // To allow the process to start
                    if (webserver.HasExited) throw new Exception("Web-server process has unexpectedly exited!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to launch web-server! If this persists, delete the file 'Launcher.xml' in the same directory as this application (" + Application.StartupPath + ")...\r\n\r\nError:\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.StackTrace, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return;
            }
        }
        private void niMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            showSettings();
        }
        private void Launcher_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Form is closing -> most likely a shutdown -> kill the launched processes
            if (db != null && !db.HasExited)
            {
                db.CloseMainWindow();
                db.Kill();
                // Read the PID from file
                string pidPath = Program.PATH_DB + "\\data\\" + Environment.MachineName + ".pid";
                if (File.Exists(pidPath))
                    try
                    {
                        Process proc = Process.GetProcessById(int.Parse(File.ReadAllText(pidPath).Trim()));
                        proc.CloseMainWindow();
                        proc.Kill();
                    }
                    catch { }
            }
            if (webserver != null && !webserver.HasExited)
            {
                webserver.CloseMainWindow();
                webserver.Kill();
            }
        }
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSettings();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
        private void openWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://" + Environment.MachineName + ":" + webPort + "/");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open web-browser! Error:\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region "Methods"
        void showSettings()
        {
            if (settingsWindow == null)
                (settingsWindow = new Settings(false)).Show();
            else
                settingsWindow.Show();
        }
        #endregion
    }
}