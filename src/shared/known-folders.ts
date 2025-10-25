import * as os from "os";
import * as path from "path";

/**
 * Known folder identifiers that work across all platforms
 */
export type CrossPlatformFolder =
  | "HOME"
  | "DESKTOP"
  | "DOCUMENTS"
  | "DOWNLOADS"
  | "MUSIC"
  | "PICTURES"
  | "VIDEOS"
  | "TEMPLATES"
  | "PUBLIC";

/**
 * Platform-specific folder identifiers
 */
export type WindowsSpecificFolder =
  | "WINDOWS:LOCALAPPDATA"
  | "WINDOWS:ROAMINGAPPDATA"
  | "WINDOWS:PROGRAMDATA"
  | "WINDOWS:STARTUP"
  | "WINDOWS:TEMP";

export type MacOSSpecificFolder =
  | "MACOS:APPLICATION_SUPPORT"
  | "MACOS:CACHES"
  | "MACOS:LIBRARY";

export type LinuxSpecificFolder = "LINUX:CONFIG" | "LINUX:DATA" | "LINUX:CACHE";

export type KnownFolder =
  | CrossPlatformFolder
  | WindowsSpecificFolder
  | MacOSSpecificFolder
  | LinuxSpecificFolder;

/**
 * Get the platform-specific path for a known folder
 * @param folder The known folder identifier
 * @returns The absolute path to the folder, or undefined if not available on this platform
 */
export function getKnownFolderPath(folder: string): string | undefined {
  const homeDir = os.homedir();

  // Cross-platform folders
  const crossPlatformFolders: Record<CrossPlatformFolder, () => string> = {
    HOME: () => homeDir,
    DESKTOP: () => getDesktopPath(homeDir),
    DOCUMENTS: () => getDocumentsPath(homeDir),
    DOWNLOADS: () => getDownloadsPath(homeDir),
    MUSIC: () => getMusicPath(homeDir),
    PICTURES: () => getPicturesPath(homeDir),
    VIDEOS: () => getVideosPath(homeDir),
    TEMPLATES: () => getTemplatesPath(homeDir),
    PUBLIC: () => getPublicPath(homeDir),
  };

  // Check if it's a cross-platform folder
  if (folder in crossPlatformFolders) {
    return crossPlatformFolders[folder as CrossPlatformFolder]();
  }

  // Handle platform-specific folders
  const platform = os.platform();

  if (folder.startsWith("WINDOWS:")) {
    if (platform === "win32") {
      return getWindowsSpecificPath(folder as WindowsSpecificFolder);
    }
    // On non-Windows platforms, Windows-specific folders are not available
    return undefined;
  }

  if (folder.startsWith("MACOS:")) {
    if (platform === "darwin") {
      return getMacOSSpecificPath(folder as MacOSSpecificFolder, homeDir);
    }
    return undefined;
  }

  if (folder.startsWith("LINUX:")) {
    if (platform === "linux") {
      return getLinuxSpecificPath(folder as LinuxSpecificFolder, homeDir);
    }
    return undefined;
  }

  // Unknown folder type
  return undefined;
}

/**
 * Get all known folder paths for the current platform
 * @returns A record of folder names to their resolved paths
 */
export function getAllKnownFolders(): Record<string, string> {
  const folders: Record<string, string> = {};

  // Add all cross-platform folders
  const crossPlatformFolders: CrossPlatformFolder[] = [
    "HOME",
    "DESKTOP",
    "DOCUMENTS",
    "DOWNLOADS",
    "MUSIC",
    "PICTURES",
    "VIDEOS",
    "TEMPLATES",
    "PUBLIC",
  ];

  for (const folder of crossPlatformFolders) {
    const folderPath = getKnownFolderPath(folder);
    if (folderPath) {
      folders[folder] = folderPath;
    }
  }

  // Add platform-specific folders
  const platform = os.platform();

  if (platform === "win32") {
    const windowsFolders: WindowsSpecificFolder[] = [
      "WINDOWS:LOCALAPPDATA",
      "WINDOWS:ROAMINGAPPDATA",
      "WINDOWS:PROGRAMDATA",
      "WINDOWS:STARTUP",
      "WINDOWS:TEMP",
    ];
    for (const folder of windowsFolders) {
      const folderPath = getKnownFolderPath(folder);
      if (folderPath) {
        folders[folder] = folderPath;
      }
    }
  } else if (platform === "darwin") {
    const macosFolders: MacOSSpecificFolder[] = [
      "MACOS:APPLICATION_SUPPORT",
      "MACOS:CACHES",
      "MACOS:LIBRARY",
    ];
    for (const folder of macosFolders) {
      const folderPath = getKnownFolderPath(folder);
      if (folderPath) {
        folders[folder] = folderPath;
      }
    }
  } else if (platform === "linux") {
    const linuxFolders: LinuxSpecificFolder[] = [
      "LINUX:CONFIG",
      "LINUX:DATA",
      "LINUX:CACHE",
    ];
    for (const folder of linuxFolders) {
      const folderPath = getKnownFolderPath(folder);
      if (folderPath) {
        folders[folder] = folderPath;
      }
    }
  }

  return folders;
}

// Platform-specific implementations

function getDesktopPath(homeDir: string): string {
  const platform = os.platform();
  if (platform === "linux") {
    return process.env.XDG_DESKTOP_DIR || path.join(homeDir, "Desktop");
  }
  return path.join(homeDir, "Desktop");
}

function getDocumentsPath(homeDir: string): string {
  const platform = os.platform();
  if (platform === "linux") {
    return process.env.XDG_DOCUMENTS_DIR || path.join(homeDir, "Documents");
  }
  return path.join(homeDir, "Documents");
}

function getDownloadsPath(homeDir: string): string {
  const platform = os.platform();
  if (platform === "linux") {
    return process.env.XDG_DOWNLOAD_DIR || path.join(homeDir, "Downloads");
  }
  return path.join(homeDir, "Downloads");
}

function getMusicPath(homeDir: string): string {
  const platform = os.platform();
  if (platform === "linux") {
    return process.env.XDG_MUSIC_DIR || path.join(homeDir, "Music");
  }
  return path.join(homeDir, "Music");
}

function getPicturesPath(homeDir: string): string {
  const platform = os.platform();
  if (platform === "linux") {
    return process.env.XDG_PICTURES_DIR || path.join(homeDir, "Pictures");
  }
  return path.join(homeDir, "Pictures");
}

function getVideosPath(homeDir: string): string {
  const platform = os.platform();
  if (platform === "linux") {
    return process.env.XDG_VIDEOS_DIR || path.join(homeDir, "Videos");
  }
  return path.join(homeDir, "Videos");
}

function getTemplatesPath(homeDir: string): string {
  const platform = os.platform();
  if (platform === "linux") {
    return process.env.XDG_TEMPLATES_DIR || path.join(homeDir, "Templates");
  }
  return path.join(homeDir, "Templates");
}

function getPublicPath(homeDir: string): string {
  const platform = os.platform();
  if (platform === "linux") {
    return process.env.XDG_PUBLICSHARE_DIR || path.join(homeDir, "Public");
  }
  return path.join(homeDir, "Public");
}

function getWindowsSpecificPath(folder: WindowsSpecificFolder): string {
  const homeDir = os.homedir();

  switch (folder) {
    case "WINDOWS:LOCALAPPDATA":
      return process.env.LOCALAPPDATA || path.join(homeDir, "AppData", "Local");
    case "WINDOWS:ROAMINGAPPDATA":
      return process.env.APPDATA || path.join(homeDir, "AppData", "Roaming");
    case "WINDOWS:PROGRAMDATA":
      return process.env.PROGRAMDATA || "C:\\ProgramData";
    case "WINDOWS:STARTUP":
      return process.env.APPDATA
        ? path.join(
            process.env.APPDATA,
            "Microsoft",
            "Windows",
            "Start Menu",
            "Programs",
            "Startup",
          )
        : path.join(
            homeDir,
            "AppData",
            "Roaming",
            "Microsoft",
            "Windows",
            "Start Menu",
            "Programs",
            "Startup",
          );
    case "WINDOWS:TEMP":
      return (
        process.env.TEMP ||
        process.env.TMP ||
        path.join(homeDir, "AppData", "Local", "Temp")
      );
    default:
      throw new Error(`Unknown Windows folder: ${folder}`);
  }
}

function getMacOSSpecificPath(
  folder: MacOSSpecificFolder,
  homeDir: string,
): string {
  switch (folder) {
    case "MACOS:APPLICATION_SUPPORT":
      return path.join(homeDir, "Library", "Application Support");
    case "MACOS:CACHES":
      return path.join(homeDir, "Library", "Caches");
    case "MACOS:LIBRARY":
      return path.join(homeDir, "Library");
    default:
      throw new Error(`Unknown macOS folder: ${folder}`);
  }
}

function getLinuxSpecificPath(
  folder: LinuxSpecificFolder,
  homeDir: string,
): string {
  switch (folder) {
    case "LINUX:CONFIG":
      return process.env.XDG_CONFIG_HOME || path.join(homeDir, ".config");
    case "LINUX:DATA":
      return process.env.XDG_DATA_HOME || path.join(homeDir, ".local", "share");
    case "LINUX:CACHE":
      return process.env.XDG_CACHE_HOME || path.join(homeDir, ".cache");
    default:
      throw new Error(`Unknown Linux folder: ${folder}`);
  }
}
