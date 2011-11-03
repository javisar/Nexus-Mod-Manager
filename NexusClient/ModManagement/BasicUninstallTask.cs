﻿using System.Collections.Generic;
using Nexus.Client.ModManagement.InstallationLog;
using Nexus.Client.Mods;
using Nexus.Client.BackgroundTasks;

namespace Nexus.Client.ModManagement
{
	/// <summary>
	/// Performs a standard mod uninstallation.
	/// </summary>
	/// <remarks>
	/// A basic uninstall uninstalls all of the changes made when the mod was installed.
	/// </remarks>
	public class BasicUninstallTask : BackgroundTask
	{
		#region Properties

		/// <summary>
		/// Gets or sets the mod being installed.
		/// </summary>
		/// <value>The mod being installed.</value>
		protected IMod Mod { get; set; }

		/// <summary>
		/// Gets the installer group to use to install mod items.
		/// </summary>
		/// <value>The installer group to use to install mod items.</value>
		protected InstallerGroup Installers { get; private set; }

		/// <summary>
		/// Gets the install log that tracks mod install info
		/// for the current game mode.
		/// </summary>
		/// <value>The install log that tracks mod install info
		/// for the current game mode.</value>
		protected IInstallLog ModInstallLog { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_modMod">The mod being installed.</param>
		/// <param name="p_igpInstallers">The utility class to use to install the mod items.</param>
		/// <param name="p_ilgModInstallLog">The install log that tracks mod install info
		/// for the current game mode</param>
		public BasicUninstallTask(IMod p_modMod, InstallerGroup p_igpInstallers, IInstallLog p_ilgModInstallLog)
		{
			Mod = p_modMod;
			Installers = p_igpInstallers;
			ModInstallLog = p_ilgModInstallLog;
		}

		#endregion

		/// <summary>
		/// Runs the uninstall task.
		/// </summary>
		/// <returns><c>true</c> if the mod was successfully
		/// uninstalled; <c>false</c> otherwise.</returns>
		public bool Execute()
		{
			bool booSuccess = UninstallFiles();
			Status = Status == TaskStatus.Cancelling ? TaskStatus.Cancelled : TaskStatus.Complete;
			OnTaskEnded(booSuccess);
			return booSuccess;
		}

		/// <summary>
		/// Performs the actual uninstallation.
		/// </summary>
		/// <returns><c>true</c> if the uninstall was successful;
		/// <c>false</c> otherwise.</returns>
		protected bool UninstallFiles()
		{
			OverallMessage = "Uninstalling Mod...";
			ShowItemProgress = true;
			OverallProgressStepSize = 1;
			ItemProgressStepSize = 1;

			IList<string> lstFiles = ModInstallLog.GetInstalledModFiles(Mod);
			IList<IniEdit> lstIniEdits = ModInstallLog.GetInstalledIniEdits(Mod);
			IList<string> lstGameSpecificValueEdits = ModInstallLog.GetInstalledGameSpecificValueEdits(Mod);
			OverallProgressMaximum = 3;

			ItemProgressMaximum = lstFiles.Count;
			ItemProgress = 0;
			ItemMessage = "Uninstalling Files...";
			foreach (string strFile in lstFiles)
			{
				if (Status == TaskStatus.Cancelling)
					return false;
				Installers.FileInstaller.UninstallDataFile(strFile);
				StepItemProgress();
			}
			StepOverallProgress();

			
			ItemProgressMaximum = lstIniEdits.Count;
			ItemProgress = 0;
			ItemMessage = "Undoing Ini Edits...";
			foreach (IniEdit iniEdit in lstIniEdits)
			{
				if (Status == TaskStatus.Cancelling)
					return false;
				Installers.IniInstaller.UneditIni(iniEdit.File, iniEdit.Section, iniEdit.Key);
				StepItemProgress();
			}
			StepOverallProgress();
						
			ItemProgressMaximum = lstGameSpecificValueEdits.Count;
			ItemProgress = 0;
			ItemMessage = "Undoing Game Specific Value Edits...";
			foreach (string strEdit in lstGameSpecificValueEdits)
			{
				if (Status == TaskStatus.Cancelling)
					return false;
				Installers.GameSpecificValueInstaller.UnEditGameSpecificValue(strEdit);
				StepItemProgress();
			}
			StepOverallProgress();

			return true;
		}
	}
}