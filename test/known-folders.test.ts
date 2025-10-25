import * as os from "os";
import * as path from "path";

import {
  getAllKnownFolders,
  getKnownFolderPath,
} from "../src/shared/known-folders";

describe("getKnownFolderPath", () => {
  const homeDir = os.homedir();
  const platform = os.platform();

  describe("Cross-platform folders", () => {
    it("should resolve HOME to user home directory", () => {
      expect(getKnownFolderPath("HOME")).toBe(homeDir);
    });

    it("should resolve DESKTOP", () => {
      const result = getKnownFolderPath("DESKTOP");
      expect(result).toBeDefined();
      expect(result).toContain("Desktop");
    });

    it("should resolve DOCUMENTS", () => {
      const result = getKnownFolderPath("DOCUMENTS");
      expect(result).toBeDefined();
      expect(result).toContain("Documents");
    });

    it("should resolve DOWNLOADS", () => {
      const result = getKnownFolderPath("DOWNLOADS");
      expect(result).toBeDefined();
      expect(result).toContain("Downloads");
    });

    it("should resolve MUSIC", () => {
      const result = getKnownFolderPath("MUSIC");
      expect(result).toBeDefined();
      expect(result).toContain("Music");
    });

    it("should resolve PICTURES", () => {
      const result = getKnownFolderPath("PICTURES");
      expect(result).toBeDefined();
      expect(result).toContain("Pictures");
    });

    it("should resolve VIDEOS", () => {
      const result = getKnownFolderPath("VIDEOS");
      expect(result).toBeDefined();
      expect(result).toContain("Videos");
    });

    it("should resolve TEMPLATES", () => {
      const result = getKnownFolderPath("TEMPLATES");
      expect(result).toBeDefined();
      expect(result).toContain("Templates");
    });

    it("should resolve PUBLIC", () => {
      const result = getKnownFolderPath("PUBLIC");
      expect(result).toBeDefined();
      expect(result).toContain("Public");
    });
  });

  describe("Platform-specific folders", () => {
    describe("Windows-specific folders", () => {
      it("should resolve WINDOWS:LOCALAPPDATA on Windows", () => {
        const result = getKnownFolderPath("WINDOWS:LOCALAPPDATA");
        if (platform === "win32") {
          expect(result).toBeDefined();
          expect(result).toContain("AppData");
        } else {
          expect(result).toBeUndefined();
        }
      });

      it("should resolve WINDOWS:ROAMINGAPPDATA on Windows", () => {
        const result = getKnownFolderPath("WINDOWS:ROAMINGAPPDATA");
        if (platform === "win32") {
          expect(result).toBeDefined();
          expect(result).toContain("AppData");
        } else {
          expect(result).toBeUndefined();
        }
      });

      it("should resolve WINDOWS:PROGRAMDATA on Windows", () => {
        const result = getKnownFolderPath("WINDOWS:PROGRAMDATA");
        if (platform === "win32") {
          expect(result).toBeDefined();
        } else {
          expect(result).toBeUndefined();
        }
      });

      it("should resolve WINDOWS:STARTUP on Windows", () => {
        const result = getKnownFolderPath("WINDOWS:STARTUP");
        if (platform === "win32") {
          expect(result).toBeDefined();
          expect(result).toContain("Startup");
        } else {
          expect(result).toBeUndefined();
        }
      });

      it("should resolve WINDOWS:TEMP on Windows", () => {
        const result = getKnownFolderPath("WINDOWS:TEMP");
        if (platform === "win32") {
          expect(result).toBeDefined();
        } else {
          expect(result).toBeUndefined();
        }
      });

      it("should not resolve Windows folders on non-Windows platforms", () => {
        if (platform !== "win32") {
          expect(getKnownFolderPath("WINDOWS:LOCALAPPDATA")).toBeUndefined();
          expect(getKnownFolderPath("WINDOWS:ROAMINGAPPDATA")).toBeUndefined();
          expect(getKnownFolderPath("WINDOWS:PROGRAMDATA")).toBeUndefined();
        }
      });
    });

    describe("macOS-specific folders", () => {
      it("should resolve MACOS:APPLICATION_SUPPORT on macOS", () => {
        const result = getKnownFolderPath("MACOS:APPLICATION_SUPPORT");
        if (platform === "darwin") {
          expect(result).toBeDefined();
          expect(result).toBe(
            path.join(homeDir, "Library", "Application Support"),
          );
        } else {
          expect(result).toBeUndefined();
        }
      });

      it("should resolve MACOS:CACHES on macOS", () => {
        const result = getKnownFolderPath("MACOS:CACHES");
        if (platform === "darwin") {
          expect(result).toBeDefined();
          expect(result).toBe(path.join(homeDir, "Library", "Caches"));
        } else {
          expect(result).toBeUndefined();
        }
      });

      it("should resolve MACOS:LIBRARY on macOS", () => {
        const result = getKnownFolderPath("MACOS:LIBRARY");
        if (platform === "darwin") {
          expect(result).toBeDefined();
          expect(result).toBe(path.join(homeDir, "Library"));
        } else {
          expect(result).toBeUndefined();
        }
      });

      it("should not resolve macOS folders on non-macOS platforms", () => {
        if (platform !== "darwin") {
          expect(
            getKnownFolderPath("MACOS:APPLICATION_SUPPORT"),
          ).toBeUndefined();
          expect(getKnownFolderPath("MACOS:CACHES")).toBeUndefined();
          expect(getKnownFolderPath("MACOS:LIBRARY")).toBeUndefined();
        }
      });
    });

    describe("Linux-specific folders", () => {
      it("should resolve LINUX:CONFIG on Linux", () => {
        const result = getKnownFolderPath("LINUX:CONFIG");
        if (platform === "linux") {
          expect(result).toBeDefined();
          // Should be XDG_CONFIG_HOME or ~/.config
          expect(
            result === process.env.XDG_CONFIG_HOME ||
              result === path.join(homeDir, ".config"),
          ).toBe(true);
        } else {
          expect(result).toBeUndefined();
        }
      });

      it("should resolve LINUX:DATA on Linux", () => {
        const result = getKnownFolderPath("LINUX:DATA");
        if (platform === "linux") {
          expect(result).toBeDefined();
          expect(
            result === process.env.XDG_DATA_HOME ||
              result === path.join(homeDir, ".local", "share"),
          ).toBe(true);
        } else {
          expect(result).toBeUndefined();
        }
      });

      it("should resolve LINUX:CACHE on Linux", () => {
        const result = getKnownFolderPath("LINUX:CACHE");
        if (platform === "linux") {
          expect(result).toBeDefined();
          expect(
            result === process.env.XDG_CACHE_HOME ||
              result === path.join(homeDir, ".cache"),
          ).toBe(true);
        } else {
          expect(result).toBeUndefined();
        }
      });

      it("should not resolve Linux folders on non-Linux platforms", () => {
        if (platform !== "linux") {
          expect(getKnownFolderPath("LINUX:CONFIG")).toBeUndefined();
          expect(getKnownFolderPath("LINUX:DATA")).toBeUndefined();
          expect(getKnownFolderPath("LINUX:CACHE")).toBeUndefined();
        }
      });
    });
  });

  describe("Error handling", () => {
    it("should return undefined for unknown folder names", () => {
      expect(getKnownFolderPath("UNKNOWN_FOLDER")).toBeUndefined();
    });

    it("should return undefined for invalid platform-specific folders", () => {
      expect(getKnownFolderPath("INVALID:FOLDER")).toBeUndefined();
    });
  });
});

describe("getAllKnownFolders", () => {
  const homeDir = os.homedir();
  const platform = os.platform();

  it("should return all cross-platform folders", () => {
    const folders = getAllKnownFolders();

    expect(folders.HOME).toBe(homeDir);
    expect(folders.DESKTOP).toBeDefined();
    expect(folders.DOCUMENTS).toBeDefined();
    expect(folders.DOWNLOADS).toBeDefined();
    expect(folders.MUSIC).toBeDefined();
    expect(folders.PICTURES).toBeDefined();
    expect(folders.VIDEOS).toBeDefined();
    expect(folders.TEMPLATES).toBeDefined();
    expect(folders.PUBLIC).toBeDefined();
  });

  it("should return platform-specific folders for current platform", () => {
    const folders = getAllKnownFolders();

    if (platform === "win32") {
      expect(folders["WINDOWS:LOCALAPPDATA"]).toBeDefined();
      expect(folders["WINDOWS:ROAMINGAPPDATA"]).toBeDefined();
      expect(folders["WINDOWS:PROGRAMDATA"]).toBeDefined();
      expect(folders["WINDOWS:STARTUP"]).toBeDefined();
      expect(folders["WINDOWS:TEMP"]).toBeDefined();

      // Should not include other platform folders
      expect(folders["MACOS:APPLICATION_SUPPORT"]).toBeUndefined();
      expect(folders["LINUX:CONFIG"]).toBeUndefined();
    } else if (platform === "darwin") {
      expect(folders["MACOS:APPLICATION_SUPPORT"]).toBeDefined();
      expect(folders["MACOS:CACHES"]).toBeDefined();
      expect(folders["MACOS:LIBRARY"]).toBeDefined();

      // Should not include other platform folders
      expect(folders["WINDOWS:LOCALAPPDATA"]).toBeUndefined();
      expect(folders["LINUX:CONFIG"]).toBeUndefined();
    } else if (platform === "linux") {
      expect(folders["LINUX:CONFIG"]).toBeDefined();
      expect(folders["LINUX:DATA"]).toBeDefined();
      expect(folders["LINUX:CACHE"]).toBeDefined();

      // Should not include other platform folders
      expect(folders["WINDOWS:LOCALAPPDATA"]).toBeUndefined();
      expect(folders["MACOS:APPLICATION_SUPPORT"]).toBeUndefined();
    }
  });

  it("should return only strings as values", () => {
    const folders = getAllKnownFolders();

    for (const [_key, value] of Object.entries(folders)) {
      expect(typeof value).toBe("string");
      expect(value.length).toBeGreaterThan(0);
    }
  });

  it("should return absolute paths", () => {
    const folders = getAllKnownFolders();

    for (const [_key, value] of Object.entries(folders)) {
      expect(path.isAbsolute(value)).toBe(true);
    }
  });
});
