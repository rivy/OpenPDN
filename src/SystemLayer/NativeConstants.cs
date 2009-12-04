/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using Microsoft.Win32.SafeHandles;
using System;

namespace PaintDotNet.SystemLayer
{
    internal static class NativeConstants
    {
        public const int MAX_PATH = 260;

        public const int CSIDL_DESKTOP_DIRECTORY = 0x0010;        // C:\Users\[user]\Desktop\
        public const int CSIDL_MYPICTURES = 0x0027;
        public const int CSIDL_PERSONAL = 0x0005;

        public const int CSIDL_PROGRAM_FILES = 0x0026;            // C:\Program Files\
        public const int CSIDL_APPDATA = 0x001a;                  // C:\Users\[user]\AppData\Roaming\
        public const int CSIDL_LOCAL_APPDATA = 0x001c;            // C:\Users\[user]\AppData\Local\
        public const int CSIDL_COMMON_DESKTOPDIRECTORY = 0x0019;  // C:\Users\All Users\Desktop

        public const int CSIDL_FLAG_CREATE = 0x8000;    // new for Win2K, or this in to force creation of folder

        public const uint SHGFP_TYPE_CURRENT = 0;
        public const uint SHGFP_TYPE_DEFAULT = 1;

        public const int BP_COMMANDLINK = 6;

        public const int CMDLS_NORMAL = 1;
        public const int CMDLS_HOT = 2;
        public const int CMDLS_PRESSED = 3;
        public const int CMDLS_DISABLED = 4;
        public const int CMDLS_DEFAULTED = 5;
        public const int CMDLS_DEFAULTED_ANIMATING = 6;

        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous = 0,
            SecurityIdentification = 1,
            SecurityImpersonation = 2,
            SecurityDelegation = 3
        }

        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation = 2
        }

        public const uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        public const uint TOKEN_DUPLICATE = 0x0002;
        public const uint TOKEN_IMPERSONATE = 0x0004;
        public const uint TOKEN_QUERY = 0x0008;
        public const uint TOKEN_QUERY_SOURCE = 0x0010;
        public const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        public const uint TOKEN_ADJUST_GROUPS = 0x0040;
        public const uint TOKEN_ADJUST_DEFAULT = 0x0080;
        public const uint TOKEN_ADJUST_SESSIONID = 0x0100;

        public const uint TOKEN_ALL_ACCESS_P = 
            STANDARD_RIGHTS_REQUIRED | 
            TOKEN_ASSIGN_PRIMARY | 
            TOKEN_DUPLICATE | 
            TOKEN_IMPERSONATE | 
            TOKEN_QUERY | 
            TOKEN_QUERY_SOURCE | 
            TOKEN_ADJUST_PRIVILEGES | 
            TOKEN_ADJUST_GROUPS | 
            TOKEN_ADJUST_DEFAULT;

        public const uint TOKEN_ALL_ACCESS = TOKEN_ALL_ACCESS_P | TOKEN_ADJUST_SESSIONID;
        public const uint TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY;
        public const uint TOKEN_WRITE = STANDARD_RIGHTS_WRITE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT;
        public const uint TOKEN_EXECUTE = STANDARD_RIGHTS_EXECUTE;

        public const uint MAXIMUM_ALLOWED = 0x02000000;
        
        public const uint PROCESS_TERMINATE = 0x0001; 
        public const uint PROCESS_CREATE_THREAD = 0x0002; 
        public const uint PROCESS_SET_SESSIONID = 0x0004; 
        public const uint PROCESS_VM_OPERATION = 0x0008; 
        public const uint PROCESS_VM_READ = 0x0010; 
        public const uint PROCESS_VM_WRITE = 0x0020; 
        public const uint PROCESS_DUP_HANDLE = 0x0040; 
        public const uint PROCESS_CREATE_PROCESS = 0x0080; 
        public const uint PROCESS_SET_QUOTA = 0x0100; 
        public const uint PROCESS_SET_INFORMATION = 0x0200; 
        public const uint PROCESS_QUERY_INFORMATION = 0x0400; 
        public const uint PROCESS_SUSPEND_RESUME = 0x0800; 
        public const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000; 
        public const uint PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFFF;

        public const uint PF_NX_ENABLED = 12;
        public const uint PF_XMMI_INSTRUCTIONS_AVAILABLE = 6;
        public const uint PF_XMMI64_INSTRUCTIONS_AVAILABLE = 10;
        public const uint PF_SSE3_INSTRUCTIONS_AVAILABLE = 13;

        public const uint CF_ENHMETAFILE = 14; 

        public static Guid BHID_Stream
        {
            get
            {
                return new Guid(0x1cebb3ab, 0x7c10, 0x499a, 0xa4, 0x17, 0x92, 0xca, 0x16, 0xc4, 0xcb, 0x83);
            }
        }

        public const string IID_IOleWindow = "00000114-0000-0000-C000-000000000046";
        public const string IID_IModalWindow = "b4db1657-70d7-485e-8e3e-6fcb5a5c1802";
        public const string IID_IFileDialog = "42f85136-db7e-439c-85f1-e4075d135fc8";
        public const string IID_IFileOpenDialog = "d57c7288-d4ad-4768-be02-9d969532d960";
        public const string IID_IFileSaveDialog = "84bccd23-5fde-4cdb-aea4-af64b83d78ab";
        public const string IID_IFileDialogEvents = "973510DB-7D7F-452B-8975-74A85828D354";
        public const string IID_IFileDialogControlEvents = "36116642-D713-4b97-9B83-7484A9D00433";
        public const string IID_IFileDialogCustomize = "8016b7b3-3d49-4504-a0aa-2a37494e606f";
        public const string IID_IShellItem = "43826D1E-E718-42EE-BC55-A1E261C37BFE";
        public const string IID_IShellItemArray = "B63EA76D-1F85-456F-A19C-48159EFA858B";
        public const string IID_IKnownFolder = "38521333-6A87-46A7-AE10-0F16706816C3";
        public const string IID_IKnownFolderManager = "44BEAAEC-24F4-4E90-B3F0-23D258FBB146";
        public const string IID_IPropertyStore = "886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99";

        public const string IID_ISequentialStream = "0c733a30-2a1c-11ce-ade5-00aa0044773d";
        public const string IID_IStream = "0000000C-0000-0000-C000-000000000046";

        public const string IID_IFileOperation = "947aab5f-0a5c-4c13-b4d6-4bf7836fc9f8";
        public const string IID_IFileOperationProgressSink = "04b0f1a7-9490-44bc-96e1-4296a31252e2";

        public const string CLSID_FileOpenDialog = "DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7";
        public const string CLSID_FileSaveDialog = "C0B4E2F3-BA21-4773-8DBA-335EC946EB8B";
        public const string CLSID_KnownFolderManager = "4df0c730-df9d-4ae3-9153-aa6b82e9795a";
        public const string CLSID_FileOperation = "3ad05575-8857-4850-9277-11b85bdb8e09";

        public enum FOF
            : uint
        {
            FOF_MULTIDESTFILES = 0x0001,
            FOF_CONFIRMMOUSE = 0x0002,
            FOF_SILENT = 0x0004,                // don't display progress UI (confirm prompts may be displayed still)
            FOF_RENAMEONCOLLISION = 0x0008,     // automatically rename the source files to avoid the collisions
            FOF_NOCONFIRMATION = 0x0010,        // don't display confirmation UI, assume "yes" for cases that can be bypassed, "no" for those that can not
            FOF_WANTMAPPINGHANDLE = 0x0020,     // Fill in SHFILEOPSTRUCT.hNameMappings
                                                // Must be freed using SHFreeNameMappings
            FOF_ALLOWUNDO = 0x0040,             // enable undo including Recycle behavior for IFileOperation::Delete()
            FOF_FILESONLY = 0x0080,             // only operate on the files (non folders), both files and folders are assumed without this
            FOF_SIMPLEPROGRESS = 0x0100,        // means don't show names of files
            FOF_NOCONFIRMMKDIR = 0x0200,        // don't dispplay confirmatino UI before making any needed directories, assume "Yes" in these cases
            FOF_NOERRORUI = 0x0400,             // don't put up error UI, other UI may be displayed, progress, confirmations
            FOF_NOCOPYSECURITYATTRIBS = 0x0800, // dont copy file security attributes (ACLs)
            FOF_NORECURSION = 0x1000,           // don't recurse into directories for operations that would recurse
            FOF_NO_CONNECTED_ELEMENTS = 0x2000, // don't operate on connected elements ("xxx_files" folders that go with .htm files)
            FOF_WANTNUKEWARNING = 0x4000,       // during delete operation, warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
            FOF_NORECURSEREPARSE = 0x8000,      // deprecated; the operations engine always does the right thing on FolderLink objects (symlinks, reparse points, folder shortcuts)

            FOF_NO_UI = (FOF_SILENT | FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_NOCONFIRMMKDIR), // don't display any UI at all

            FOFX_NOSKIPJUNCTIONS = 0x00010000,        // Don't avoid binding to junctions (like Task folder, Recycle-Bin)
            FOFX_PREFERHARDLINK = 0x00020000,         // Create hard link if possible
            FOFX_SHOWELEVATIONPROMPT = 0x00040000,    // Show elevation prompts when error UI is disabled (use with FOF_NOERRORUI)
            FOFX_EARLYFAILURE = 0x00100000,           // Fail operation as soon as a single error occurs rather than trying to process other items (applies only when using FOF_NOERRORUI)
            FOFX_PRESERVEFILEEXTENSIONS = 0x00200000, // Rename collisions preserve file extns (use with FOF_RENAMEONCOLLISION)
            FOFX_KEEPNEWERFILE = 0x00400000,          // Keep newer file on naming conflicts
            FOFX_NOCOPYHOOKS = 0x00800000,            // Don't use copy hooks
            FOFX_NOMINIMIZEBOX = 0x01000000,          // Don't allow minimizing the progress dialog
            FOFX_MOVEACLSACROSSVOLUMES = 0x02000000,  // Copy security information when performing a cross-volume move operation
            FOFX_DONTDISPLAYSOURCEPATH = 0x04000000,  // Don't display the path of source file in progress dialog
            FOFX_DONTDISPLAYDESTPATH = 0x08000000,    // Don't display the path of destination file in progress dialog
        }
        
        public enum STATFLAG
            : uint
        {
            STATFLAG_DEFAULT = 0,
            STATFLAG_NONAME = 1,
            STATFLAG_NOOPEN = 2
        }

        public enum STGTY
            : uint
        {
            STGTY_STORAGE = 1,
            STGTY_STREAM = 2,
            STGTY_LOCKBYTES = 3,
            STGTY_PROPERTY = 4
        }

        [Flags]
        public enum STGC
            : uint
        {
            STGC_DEFAULT = 0,
            STGC_OVERWRITE = 1,
            STGC_ONLYIFCURRENT = 2,
            STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 4,
            STGC_CONSOLIDATE = 8
        }

        public enum CDCONTROLSTATE
        {
            CDCS_INACTIVE = 0x00000000,
            CDCS_ENABLED = 0x00000001,
            CDCS_VISIBLE = 0x00000002
        }

        public enum FFFP_MODE
        {
            FFFP_EXACTMATCH,
            FFFP_NEARESTPARENTMATCH
        }

        public enum SIATTRIBFLAGS
        {
            SIATTRIBFLAGS_AND = 0x00000001, // if multiple items and the attirbutes together.
            SIATTRIBFLAGS_OR = 0x00000002, // if multiple items or the attributes together.
            SIATTRIBFLAGS_APPCOMPAT = 0x00000003, // Call GetAttributes directly on the ShellFolder for multiple attributes
        }

        public enum SIGDN : uint
        {
            SIGDN_NORMALDISPLAY = 0x00000000,                 // SHGDN_NORMAL
            SIGDN_PARENTRELATIVEPARSING = 0x80018001,         // SHGDN_INFOLDER | SHGDN_FORPARSING
            SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,        // SHGDN_FORPARSING
            SIGDN_PARENTRELATIVEEDITING = 0x80031001,         // SHGDN_INFOLDER | SHGDN_FOREDITING
            SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,        // SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
            SIGDN_FILESYSPATH = 0x80058000,                   // SHGDN_FORPARSING
            SIGDN_URL = 0x80068000,                           // SHGDN_FORPARSING
            SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,   // SHGDN_INFOLDER | SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
            SIGDN_PARENTRELATIVE = 0x80080001                 // SHGDN_INFOLDER
        }

        public const uint DROPEFFECT_COPY	= 1;
        public const uint DROPEFFECT_MOVE = 2;
        public const uint DROPEFFECT_LINK = 4;

        [Flags]
        public enum SFGAO : uint
        {
             SFGAO_CANCOPY = DROPEFFECT_COPY,        // Objects can be copied (0x1)
             SFGAO_CANMOVE = DROPEFFECT_MOVE,        // Objects can be moved (0x2)
             SFGAO_CANLINK = DROPEFFECT_LINK,        // Objects can be linked (0x4)
             SFGAO_STORAGE = 0x00000008,             // supports BindToObject(IID_IStorage)
             SFGAO_CANRENAME = 0x00000010,           // Objects can be renamed
             SFGAO_CANDELETE = 0x00000020,           // Objects can be deleted
             SFGAO_HASPROPSHEET = 0x00000040,        // Objects have property sheets
             SFGAO_DROPTARGET = 0x00000100,          // Objects are drop target
             SFGAO_CAPABILITYMASK = 0x00000177,
             SFGAO_ENCRYPTED = 0x00002000,           // Object is encrypted (use alt color)
             SFGAO_ISSLOW = 0x00004000,              // 'Slow' object
             SFGAO_GHOSTED = 0x00008000,             // Ghosted icon
             SFGAO_LINK = 0x00010000,                // Shortcut (link)
             SFGAO_SHARE = 0x00020000,               // Shared
             SFGAO_READONLY = 0x00040000,            // Read-only
             SFGAO_HIDDEN = 0x00080000,              // Hidden object
             SFGAO_DISPLAYATTRMASK = 0x000FC000,
             SFGAO_FILESYSANCESTOR = 0x10000000,     // May contain children with SFGAO_FILESYSTEM
             SFGAO_FOLDER = 0x20000000,              // Support BindToObject(IID_IShellFolder)
             SFGAO_FILESYSTEM = 0x40000000,          // Is a win32 file system object (file/folder/root)
             SFGAO_HASSUBFOLDER = 0x80000000,        // May contain children with SFGAO_FOLDER (may be slow)
             SFGAO_CONTENTSMASK = 0x80000000,
             SFGAO_VALIDATE = 0x01000000,            // Invalidate cached information (may be slow)
             SFGAO_REMOVABLE = 0x02000000,           // Is this removeable media?
             SFGAO_COMPRESSED = 0x04000000,          // Object is compressed (use alt color)
             SFGAO_BROWSABLE = 0x08000000,           // Supports IShellFolder, but only implements CreateViewObject() (non-folder view)
             SFGAO_NONENUMERATED = 0x00100000,       // Is a non-enumerated object (should be hidden)
             SFGAO_NEWCONTENT = 0x00200000,          // Should show bold in explorer tree
             SFGAO_STREAM = 0x00400000,              // Supports BindToObject(IID_IStream)
             SFGAO_CANMONIKER = 0x00400000,          // Obsolete
             SFGAO_HASSTORAGE = 0x00400000,          // Obsolete
             SFGAO_STORAGEANCESTOR = 0x00800000,     // May contain children with SFGAO_STORAGE or SFGAO_STREAM
             SFGAO_STORAGECAPMASK = 0x70C50008,      // For determining storage capabilities, ie for open/save semantics
             SFGAO_PKEYSFGAOMASK = 0x81044010        // Attributes that are masked out for PKEY_SFGAOFlags because they are considered to cause slow calculations or lack context (SFGAO_VALIDATE | SFGAO_ISSLOW | SFGAO_HASSUBFOLDER and others)
        }

        public enum FDE_OVERWRITE_RESPONSE
        {
            FDEOR_DEFAULT = 0x00000000,
            FDEOR_ACCEPT = 0x00000001,
            FDEOR_REFUSE = 0x00000002
        }

        public enum FDE_SHAREVIOLATION_RESPONSE
        {
            FDESVR_DEFAULT = 0x00000000,
            FDESVR_ACCEPT = 0x00000001,
            FDESVR_REFUSE = 0x00000002
        }

        public enum FDAP
        {
            FDAP_BOTTOM = 0x00000000,
            FDAP_TOP = 0x00000001,
        }

        [Flags]
        public enum FOS : uint
        {
            FOS_OVERWRITEPROMPT = 0x00000002,
            FOS_STRICTFILETYPES = 0x00000004,
            FOS_NOCHANGEDIR = 0x00000008,
            FOS_PICKFOLDERS = 0x00000020,
            FOS_FORCEFILESYSTEM = 0x00000040, // Ensure that items returned are filesystem items.
            FOS_ALLNONSTORAGEITEMS = 0x00000080, // Allow choosing items that have no storage.
            FOS_NOVALIDATE = 0x00000100,
            FOS_ALLOWMULTISELECT = 0x00000200,
            FOS_PATHMUSTEXIST = 0x00000800,
            FOS_FILEMUSTEXIST = 0x00001000,
            FOS_CREATEPROMPT = 0x00002000,
            FOS_SHAREAWARE = 0x00004000,
            FOS_NOREADONLYRETURN = 0x00008000,
            FOS_NOTESTFILECREATE = 0x00010000,
            FOS_HIDEMRUPLACES = 0x00020000,
            FOS_HIDEPINNEDPLACES = 0x00040000,
            FOS_NODEREFERENCELINKS = 0x00100000,
            FOS_DONTADDTORECENT = 0x02000000,
            FOS_FORCESHOWHIDDEN = 0x10000000,
            FOS_DEFAULTNOMINIMODE = 0x20000000
        }

        public enum KF_CATEGORY
        {
            KF_CATEGORY_VIRTUAL = 0x00000001,
            KF_CATEGORY_FIXED = 0x00000002,
            KF_CATEGORY_COMMON = 0x00000003,
            KF_CATEGORY_PERUSER = 0x00000004
        }

        [Flags]
        public enum KF_DEFINITION_FLAGS
        {
            KFDF_PERSONALIZE = 0x00000001,
            KFDF_LOCAL_REDIRECT_ONLY = 0x00000002,
            KFDF_ROAMABLE = 0x00000004,
        }

        public const uint DWMWA_NCRENDERING_ENABLED = 1;           // [get] Is non-client rendering enabled/disabled
        public const uint DWMWA_NCRENDERING_POLICY = 2;            // [set] Non-client rendering policy
        public const uint DWMWA_TRANSITIONS_FORCEDISABLED = 3;     // [set] Potentially enable/forcibly disable transitions
        public const uint DWMWA_ALLOW_NCPAINT = 4;                 // [set] Allow contents rendered in the non-client area to be visible on the DWM-drawn frame.
        public const uint DWMWA_CAPTION_BUTTON_BOUNDS = 5;         // [get] Bounds of the caption button area in window-relative space.
        public const uint DWMWA_NONCLIENT_RTL_LAYOUT = 6;          // [set] Is non-client content RTL mirrored
        public const uint DWMWA_FORCE_ICONIC_REPRESENTATION = 7;   // [set] Force this window to display iconic thumbnails.
        public const uint DWMWA_FLIP3D_POLICY = 8;                 // [set] Designates how Flip3D will treat the window.
        public const uint DWMWA_EXTENDED_FRAME_BOUNDS = 9;         // [get] Gets the extended frame bounds rectangle in screen space
        public const uint DWMWA_LAST = 10;

        public const uint DWMNCRP_USEWINDOWSTYLE = 0;
        public const uint DWMNCRP_DISABLED = 1;
        public const uint DWMNCRP_ENABLED = 2;
        public const uint DWMNCRP_LAST = 3;

        public const byte VER_EQUAL = 1;
        public const byte VER_GREATER = 2;
        public const byte VER_GREATER_EQUAL = 3;
        public const byte VER_LESS = 4;
        public const byte VER_LESS_EQUAL = 5;
        public const byte VER_AND = 6;
        public const byte VER_OR = 7;

        public const uint VER_CONDITION_MASK = 7;
        public const uint VER_NUM_BITS_PER_CONDITION_MASK = 3;

        public const uint VER_MINORVERSION = 0x0000001;
        public const uint VER_MAJORVERSION = 0x0000002;
        public const uint VER_BUILDNUMBER = 0x0000004;
        public const uint VER_PLATFORMID = 0x0000008;
        public const uint VER_SERVICEPACKMINOR = 0x0000010;
        public const uint VER_SERVICEPACKMAJOR = 0x0000020;
        public const uint VER_SUITENAME = 0x0000040;
        public const uint VER_PRODUCT_TYPE = 0x0000080;

        public const uint VER_PLATFORM_WIN32s = 0;
        public const uint VER_PLATFORM_WIN32_WINDOWS = 1;
        public const uint VER_PLATFORM_WIN32_NT = 2;

        public const int THREAD_MODE_BACKGROUND_BEGIN = 0x10000;
        public const int THREAD_MODE_BACKGROUND_END = 0x20000;

        private static uint CTL_CODE(uint deviceType, uint function, uint method, uint access)
        {
            return (deviceType << 16) | (access << 14) | (function << 2) | method;
        }

        public const uint FILE_DEVICE_FILE_SYSTEM = 0x00000009;
        public const uint METHOD_BUFFERED = 0;

        public static readonly uint FSCTL_SET_COMPRESSION =
            CTL_CODE(FILE_DEVICE_FILE_SYSTEM, 16, METHOD_BUFFERED, FILE_READ_DATA | FILE_WRITE_DATA);

        public static ushort COMPRESSION_FORMAT_DEFAULT = 1;

        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_FORCEMINIMIZE = 11;
        public const int SW_MAX = 11;

        public const uint MF_BYCOMMAND = 0;
        public const uint MF_GRAYED = 1;
        public const uint MF_DISABLED = 2;
        public const uint SC_CLOSE = 0xf060;

        public const uint SEE_MASK_CLASSNAME = 0x00000001;
        public const uint SEE_MASK_CLASSKEY = 0x00000003;
        public const uint SEE_MASK_IDLIST = 0x00000004;
        public const uint SEE_MASK_INVOKEIDLIST = 0x0000000c;
        public const uint SEE_MASK_ICON = 0x00000010;
        public const uint SEE_MASK_HOTKEY = 0x00000020;
        public const uint SEE_MASK_NOCLOSEPROCESS = 0x00000040;
        public const uint SEE_MASK_CONNECTNETDRV = 0x00000080;
        public const uint SEE_MASK_FLAG_DDEWAIT = 0x00000100;
        public const uint SEE_MASK_DOENVSUBST = 0x00000200;
        public const uint SEE_MASK_FLAG_NO_UI = 0x00000400;
        public const uint SEE_MASK_UNICODE = 0x00004000;
        public const uint SEE_MASK_NO_CONSOLE = 0x00008000;
        public const uint SEE_MASK_ASYNCOK = 0x00100000;
        public const uint SEE_MASK_HMONITOR = 0x00200000;
        public const uint SEE_MASK_NOZONECHECKS = 0x00800000;
        public const uint SEE_MASK_NOQUERYCLASSSTORE = 0x01000000;
        public const uint SEE_MASK_WAITFORINPUTIDLE = 0x02000000;
        public const uint SEE_MASK_FLAG_LOG_USAGE = 0x04000000;

        public const uint SHARD_PIDL = 0x00000001;
        public const uint SHARD_PATHA = 0x00000002;
        public const uint SHARD_PATHW = 0x00000003;

        public const uint VER_NT_WORKSTATION = 0x0000001;
        public const uint VER_NT_DOMAIN_CONTROLLER = 0x0000002;
        public const uint VER_NT_SERVER = 0x0000003;

        public const uint LWA_COLORKEY = 0x00000001;
        public const uint LWA_ALPHA = 0x00000002;
        public const uint WS_EX_LAYERED = 0x00080000;

        public const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
        public const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
        public const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
        public const ushort PROCESSOR_ARCHITECTURE_UNKNOWN = 0xFFFF;

        public const uint SHVIEW_THUMBNAIL = 0x702d;

        public const uint MA_ACTIVATE = 1;
        public const uint MA_ACTIVATEANDEAT = 2;
        public const uint MA_NOACTIVATE = 3;
        public const uint MA_NOACTIVATEANDEAT = 4;

        public const uint IDI_APPLICATION = 32512;

        public const int ERROR_SUCCESS = 0;
        public const int ERROR_ALREADY_EXISTS = 183;
        public const int ERROR_CANCELLED = 1223;
        public const int ERROR_IO_PENDING = 0x3e5;
        public const int ERROR_NO_MORE_ITEMS = 259;
        public const int ERROR_TIMEOUT = 1460;

        public const uint DIGCF_PRESENT = 2;

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        public const int GWLP_WNDPROC = -4;
        public const int GWLP_HINSTANCE = -6;
        public const int GWLP_HWNDPARENT = -8;
        public const int GWLP_USERDATA = -21;
        public const int GWLP_ID = -12;

        public const uint PBS_SMOOTH = 0x01;
        public const uint PBS_MARQUEE = 0x08;
        public const int PBM_SETMARQUEE = WM_USER + 10;

        public const int SBM_SETPOS = 0x00E0;
        public const int SBM_SETRANGE = 0x00E2;
        public const int SBM_SETRANGEREDRAW = 0x00E6;
        public const int SBM_SETSCROLLINFO = 0x00E9;

        public const int BCM_FIRST = 0x1600;
        public const int BCM_SETSHIELD = BCM_FIRST + 0x000C;

        public const int CB_SHOWDROPDOWN = 0x014f;

        public const uint WM_COMMAND = 0x111;
        public const uint WM_MOUSEACTIVATE = 0x21;
        public const uint WM_COPYDATA = 0x004a;

        public const uint SMTO_NORMAL = 0x0000;
        public const uint SMTO_BLOCK = 0x0001;
        public const uint SMTO_ABORTIFHUNG = 0x0002;
        public const uint SMTO_NOTIMEOUTIFNOTHUNG = 0x0008;

        public const int WM_USER = 0x400;
        public const int WM_HSCROLL = 0x114;
        public const int WM_VSCROLL = 0x115;
        public const int WM_SETFOCUS = 7;
        public const int WM_QUERYENDSESSION = 0x0011;
        public const int WM_ACTIVATE = 0x006;
        public const int WM_ACTIVATEAPP = 0x01C;
        public const int WM_PAINT = 0x000f;
        public const int WM_NCPAINT = 0x0085;
        public const int WM_NCACTIVATE = 0x086;
        public const int WM_SETREDRAW = 0x000B;

        public const uint WS_VSCROLL = 0x00200000;
        public const uint WS_HSCROLL = 0x00100000;

        public const uint BS_MULTILINE = 0x00002000;

        public const uint ANSI_CHARSET = 0;
        public const uint DEFAULT_CHARSET = 1;
        public const uint SYMBOL_CHARSET = 2;
        public const uint SHIFTJIS_CHARSET = 128;
        public const uint HANGEUL_CHARSET = 129;
        public const uint HANGUL_CHARSET = 129;
        public const uint GB2312_CHARSET = 134;
        public const uint CHINESEBIG5_CHARSET = 136;
        public const uint OEM_CHARSET = 255;
        public const uint JOHAB_CHARSET = 130;
        public const uint HEBREW_CHARSET = 177;
        public const uint ARABIC_CHARSET = 178;
        public const uint GREEK_CHARSET = 161;
        public const uint TURKISH_CHARSET = 162;
        public const uint VIETNAMESE_CHARSET = 163;
        public const uint THAI_CHARSET = 222;
        public const uint EASTEUROPE_CHARSET = 238;
        public const uint RUSSIAN_CHARSET = 204;
        public const uint MAC_CHARSET = 77;
        public const uint BALTIC_CHARSET = 186;

        public const uint SPI_GETBEEP = 0x0001;
        public const uint SPI_SETBEEP = 0x0002;
        public const uint SPI_GETMOUSE = 0x0003;
        public const uint SPI_SETMOUSE = 0x0004;
        public const uint SPI_GETBORDER = 0x0005;
        public const uint SPI_SETBORDER = 0x0006;
        public const uint SPI_GETKEYBOARDSPEED = 0x000A;
        public const uint SPI_SETKEYBOARDSPEED = 0x000B;
        public const uint SPI_LANGDRIVER = 0x000C;
        public const uint SPI_ICONHORIZONTALSPACING = 0x000D;
        public const uint SPI_GETSCREENSAVETIMEOUT = 0x000E;
        public const uint SPI_SETSCREENSAVETIMEOUT = 0x000F;
        public const uint SPI_GETSCREENSAVEACTIVE = 0x0010;
        public const uint SPI_SETSCREENSAVEACTIVE = 0x0011;
        public const uint SPI_GETGRIDGRANULARITY = 0x0012;
        public const uint SPI_SETGRIDGRANULARITY = 0x0013;
        public const uint SPI_SETDESKWALLPAPER = 0x0014;
        public const uint SPI_SETDESKPATTERN = 0x0015;
        public const uint SPI_GETKEYBOARDDELAY = 0x0016;
        public const uint SPI_SETKEYBOARDDELAY = 0x0017;
        public const uint SPI_ICONVERTICALSPACING = 0x0018;
        public const uint SPI_GETICONTITLEWRAP = 0x0019;
        public const uint SPI_SETICONTITLEWRAP = 0x001A;
        public const uint SPI_GETMENUDROPALIGNMENT = 0x001B;
        public const uint SPI_SETMENUDROPALIGNMENT = 0x001C;
        public const uint SPI_SETDOUBLECLKWIDTH = 0x001D;
        public const uint SPI_SETDOUBLECLKHEIGHT = 0x001E;
        public const uint SPI_GETICONTITLELOGFONT = 0x001F;
        public const uint SPI_SETDOUBLECLICKTIME = 0x0020;
        public const uint SPI_SETMOUSEBUTTONSWAP = 0x0021;
        public const uint SPI_SETICONTITLELOGFONT = 0x0022;
        public const uint SPI_GETFASTTASKSWITCH = 0x0023;
        public const uint SPI_SETFASTTASKSWITCH = 0x0024;
        public const uint SPI_SETDRAGFULLWINDOWS = 0x0025;
        public const uint SPI_GETDRAGFULLWINDOWS = 0x0026;
        public const uint SPI_GETNONCLIENTMETRICS = 0x0029;
        public const uint SPI_SETNONCLIENTMETRICS = 0x002A;
        public const uint SPI_GETMINIMIZEDMETRICS = 0x002B;
        public const uint SPI_SETMINIMIZEDMETRICS = 0x002C;
        public const uint SPI_GETICONMETRICS = 0x002D;
        public const uint SPI_SETICONMETRICS = 0x002E;
        public const uint SPI_SETWORKAREA = 0x002F;
        public const uint SPI_GETWORKAREA = 0x0030;
        public const uint SPI_SETPENWINDOWS = 0x0031;
        public const uint SPI_GETHIGHCONTRAST = 0x0042;
        public const uint SPI_SETHIGHCONTRAST = 0x0043;
        public const uint SPI_GETKEYBOARDPREF = 0x0044;
        public const uint SPI_SETKEYBOARDPREF = 0x0045;
        public const uint SPI_GETSCREENREADER = 0x0046;
        public const uint SPI_SETSCREENREADER = 0x0047;
        public const uint SPI_GETANIMATION = 0x0048;
        public const uint SPI_SETANIMATION = 0x0049;
        public const uint SPI_GETFONTSMOOTHING = 0x004A;
        public const uint SPI_SETFONTSMOOTHING = 0x004B;
        public const uint SPI_SETDRAGWIDTH = 0x004C;
        public const uint SPI_SETDRAGHEIGHT = 0x004D;
        public const uint SPI_SETHANDHELD = 0x004E;
        public const uint SPI_GETLOWPOWERTIMEOUT = 0x004F;
        public const uint SPI_GETPOWEROFFTIMEOUT = 0x0050;
        public const uint SPI_SETLOWPOWERTIMEOUT = 0x0051;
        public const uint SPI_SETPOWEROFFTIMEOUT = 0x0052;
        public const uint SPI_GETLOWPOWERACTIVE = 0x0053;
        public const uint SPI_GETPOWEROFFACTIVE = 0x0054;
        public const uint SPI_SETLOWPOWERACTIVE = 0x0055;
        public const uint SPI_SETPOWEROFFACTIVE = 0x0056;
        public const uint SPI_SETCURSORS = 0x0057;
        public const uint SPI_SETICONS = 0x0058;
        public const uint SPI_GETDEFAULTINPUTLANG = 0x0059;
        public const uint SPI_SETDEFAULTINPUTLANG = 0x005A;
        public const uint SPI_SETLANGTOGGLE = 0x005B;
        public const uint SPI_GETWINDOWSEXTENSION = 0x005C;
        public const uint SPI_SETMOUSETRAILS = 0x005D;
        public const uint SPI_GETMOUSETRAILS = 0x005E;
        public const uint SPI_SETSCREENSAVERRUNNING = 0x0061;
        public const uint SPI_SCREENSAVERRUNNING = SPI_SETSCREENSAVERRUNNING;
        public const uint SPI_GETFILTERKEYS = 0x0032;
        public const uint SPI_SETFILTERKEYS = 0x0033;
        public const uint SPI_GETTOGGLEKEYS = 0x0034;
        public const uint SPI_SETTOGGLEKEYS = 0x0035;
        public const uint SPI_GETMOUSEKEYS = 0x0036;
        public const uint SPI_SETMOUSEKEYS = 0x0037;
        public const uint SPI_GETSHOWSOUNDS = 0x0038;
        public const uint SPI_SETSHOWSOUNDS = 0x0039;
        public const uint SPI_GETSTICKYKEYS = 0x003A;
        public const uint SPI_SETSTICKYKEYS = 0x003B;
        public const uint SPI_GETACCESSTIMEOUT = 0x003C;
        public const uint SPI_SETACCESSTIMEOUT = 0x003D;
        public const uint SPI_GETSERIALKEYS = 0x003E;
        public const uint SPI_SETSERIALKEYS = 0x003F;
        public const uint SPI_GETSOUNDSENTRY = 0x0040;
        public const uint SPI_SETSOUNDSENTRY = 0x0041;
        public const uint SPI_GETSNAPTODEFBUTTON = 0x005F;
        public const uint SPI_SETSNAPTODEFBUTTON = 0x0060;
        public const uint SPI_GETMOUSEHOVERWIDTH = 0x0062;
        public const uint SPI_SETMOUSEHOVERWIDTH = 0x0063;
        public const uint SPI_GETMOUSEHOVERHEIGHT = 0x0064;
        public const uint SPI_SETMOUSEHOVERHEIGHT = 0x0065;
        public const uint SPI_GETMOUSEHOVERTIME = 0x0066;
        public const uint SPI_SETMOUSEHOVERTIME = 0x0067;
        public const uint SPI_GETWHEELSCROLLLINES = 0x0068;
        public const uint SPI_SETWHEELSCROLLLINES = 0x0069;
        public const uint SPI_GETMENUSHOWDELAY = 0x006A;
        public const uint SPI_SETMENUSHOWDELAY = 0x006B;
        public const uint SPI_GETSHOWIMEUI = 0x006E;
        public const uint SPI_SETSHOWIMEUI = 0x006F;
        public const uint SPI_GETMOUSESPEED = 0x0070;
        public const uint SPI_SETMOUSESPEED = 0x0071;
        public const uint SPI_GETSCREENSAVERRUNNING = 0x0072;
        public const uint SPI_GETDESKWALLPAPER = 0x0073;
        public const uint SPI_GETACTIVEWINDOWTRACKING = 0x1000;
        public const uint SPI_SETACTIVEWINDOWTRACKING = 0x1001;
        public const uint SPI_GETMENUANIMATION = 0x1002;
        public const uint SPI_SETMENUANIMATION = 0x1003;
        public const uint SPI_GETCOMBOBOXANIMATION = 0x1004;
        public const uint SPI_SETCOMBOBOXANIMATION = 0x1005;
        public const uint SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006;
        public const uint SPI_SETLISTBOXSMOOTHSCROLLING = 0x1007;
        public const uint SPI_GETGRADIENTCAPTIONS = 0x1008;
        public const uint SPI_SETGRADIENTCAPTIONS = 0x1009;
        public const uint SPI_GETKEYBOARDCUES = 0x100A;
        public const uint SPI_SETKEYBOARDCUES = 0x100B;
        public const uint SPI_GETMENUUNDERLINES = SPI_GETKEYBOARDCUES;
        public const uint SPI_SETMENUUNDERLINES = SPI_SETKEYBOARDCUES;
        public const uint SPI_GETACTIVEWNDTRKZORDER = 0x100C;
        public const uint SPI_SETACTIVEWNDTRKZORDER = 0x100D;
        public const uint SPI_GETHOTTRACKING = 0x100E;
        public const uint SPI_SETHOTTRACKING = 0x100F;
        public const uint SPI_GETMENUFADE = 0x1012;
        public const uint SPI_SETMENUFADE = 0x1013;
        public const uint SPI_GETSELECTIONFADE = 0x1014;
        public const uint SPI_SETSELECTIONFADE = 0x1015;
        public const uint SPI_GETTOOLTIPANIMATION = 0x1016;
        public const uint SPI_SETTOOLTIPANIMATION = 0x1017;
        public const uint SPI_GETTOOLTIPFADE = 0x1018;
        public const uint SPI_SETTOOLTIPFADE = 0x1019;
        public const uint SPI_GETCURSORSHADOW = 0x101A;
        public const uint SPI_SETCURSORSHADOW = 0x101B;
        public const uint SPI_GETMOUSESONAR = 0x101C;
        public const uint SPI_SETMOUSESONAR = 0x101D;
        public const uint SPI_GETMOUSECLICKLOCK = 0x101E;
        public const uint SPI_SETMOUSECLICKLOCK = 0x101F;
        public const uint SPI_GETMOUSEVANISH = 0x1020;
        public const uint SPI_SETMOUSEVANISH = 0x1021;
        public const uint SPI_GETFLATMENU = 0x1022;
        public const uint SPI_SETFLATMENU = 0x1023;
        public const uint SPI_GETDROPSHADOW = 0x1024;
        public const uint SPI_SETDROPSHADOW = 0x1025;
        public const uint SPI_GETBLOCKSENDINPUTRESETS = 0x1026;
        public const uint SPI_SETBLOCKSENDINPUTRESETS = 0x1027;
        public const uint SPI_GETUIEFFECTS = 0x103E;
        public const uint SPI_SETUIEFFECTS = 0x103F;
        public const uint SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
        public const uint SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
        public const uint SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002;
        public const uint SPI_SETACTIVEWNDTRKTIMEOUT = 0x2003;
        public const uint SPI_GETFOREGROUNDFLASHCOUNT = 0x2004;
        public const uint SPI_SETFOREGROUNDFLASHCOUNT = 0x2005;
        public const uint SPI_GETCARETWIDTH = 0x2006;
        public const uint SPI_SETCARETWIDTH = 0x2007;
        public const uint SPI_GETMOUSECLICKLOCKTIME = 0x2008;
        public const uint SPI_SETMOUSECLICKLOCKTIME = 0x2009;
        public const uint SPI_GETFONTSMOOTHINGTYPE = 0x200A;
        public const uint SPI_SETFONTSMOOTHINGTYPE = 0x200B;
        public const uint SPI_GETFONTSMOOTHINGCONTRAST = 0x200C;
        public const uint SPI_SETFONTSMOOTHINGCONTRAST = 0x200D;
        public const uint SPI_GETFOCUSBORDERWIDTH = 0x200E;
        public const uint SPI_SETFOCUSBORDERWIDTH = 0x200F;
        public const uint SPI_GETFOCUSBORDERHEIGHT = 0x2010;
        public const uint SPI_SETFOCUSBORDERHEIGHT = 0x2011;
        public const uint SPI_GETFONTSMOOTHINGORIENTATION = 0x2012;
        public const uint SPI_SETFONTSMOOTHINGORIENTATION = 0x2013;

        public const uint INFINITE = 0xffffffff;
        public const uint STATUS_WAIT_0 = 0;
        public const uint STATUS_ABANDONED_WAIT_0 = 0x80;
        public const uint WAIT_FAILED = 0xffffffff;
        public const uint WAIT_TIMEOUT = 258;
        public const uint WAIT_ABANDONED = STATUS_ABANDONED_WAIT_0 + 0;
        public const uint WAIT_OBJECT_0 = STATUS_WAIT_0 + 0;
        public const uint WAIT_ABANDONED_0 = STATUS_ABANDONED_WAIT_0 + 0;
        public const uint STATUS_USER_APC = 0x000000C0;
        public const uint WAIT_IO_COMPLETION = STATUS_USER_APC;

        public const int SM_REMOTESESSION = 0x1000;
        public const int WM_WTSSESSION_CHANGE = 0x2b1;
        public const int WM_MOVING = 0x0216;
        public const uint NOTIFY_FOR_ALL_SESSIONS = 1;
        public const uint NOTIFY_FOR_THIS_SESSION = 0;

        public const int BP_PUSHBUTTON = 1;
        public const int PBS_NORMAL = 1;
        public const int PBS_HOT = 2;
        public const int PBS_PRESSED = 3;
        public const int PBS_DISABLED = 4;
        public const int PBS_DEFAULTED = 5;

        public const int PS_SOLID = 0;
        public const int PS_DASH = 1;             /* -------  */
        public const int PS_DOT = 2;              /* .......  */
        public const int PS_DASHDOT = 3;          /* _._._._  */
        public const int PS_DASHDOTDOT = 4;       /* _.._.._  */
        public const int PS_NULL = 5;
        public const int PS_INSIDEFRAME = 6;
        public const int PS_USERSTYLE = 7;
        public const int PS_ALTERNATE = 8;

        public const int PS_ENDCAP_ROUND = 0x00000000;
        public const int PS_ENDCAP_SQUARE = 0x00000100;
        public const int PS_ENDCAP_FLAT = 0x00000200;
        public const int PS_ENDCAP_MASK = 0x00000F00;

        public const int PS_JOIN_ROUND = 0x00000000;
        public const int PS_JOIN_BEVEL = 0x00001000;
        public const int PS_JOIN_MITER = 0x00002000;
        public const int PS_JOIN_MASK = 0x0000F000;

        public const int PS_COSMETIC = 0x00000000;
        public const int PS_GEOMETRIC = 0x00010000;
        public const int PS_TYPE_MASK = 0x000F0000;

        public const int BS_SOLID = 0;
        public const int BS_NULL = 1;
        public const int BS_HOLLOW = BS_NULL;
        public const int BS_HATCHED = 2;
        public const int BS_PATTERN = 3;
        public const int BS_INDEXED = 4;
        public const int BS_DIBPATTERN = 5;
        public const int BS_DIBPATTERNPT = 6;
        public const int BS_PATTERN8X8 = 7;
        public const int BS_DIBPATTERN8X8 = 8;
        public const int BS_MONOPATTERN = 9;

        public const uint SRCCOPY = 0x00CC0020;     /* dest = source  */
        public const uint SRCPAINT = 0x00EE0086;    /* dest = source OR dest */
        public const uint SRCAND = 0x008800C6;      /* dest = source AND dest */
        public const uint SRCINVERT = 0x00660046;   /* dest = source XOR dest */
        public const uint SRCERASE = 0x00440328;    /* dest = source AND (NOT dest ) */
        public const uint NOTSRCCOPY = 0x00330008;  /* dest = (NOT source) */
        public const uint NOTSRCERASE = 0x001100A6; /* dest = (NOT src) AND (NOT dest) */
        public const uint MERGECOPY = 0x00C000CA;   /* dest = (source AND pattern) */
        public const uint MERGEPAINT = 0x00BB0226;  /* dest = (NOT source) OR dest */
        public const uint PATCOPY = 0x00F00021;     /* dest = pattern  */
        public const uint PATPAINT = 0x00FB0A09;    /* dest = DPSnoo  */
        public const uint PATINVERT = 0x005A0049;   /* dest = pattern XOR dest */
        public const uint DSTINVERT = 0x00550009;   /* dest = (NOT dest) */
        public const uint BLACKNESS = 0x00000042;   /* dest = BLACK  */
        public const uint WHITENESS = 0x00FF0062;   /* dest = WHITE  */

        public const uint NOMIRRORBITMAP = 0x80000000; /* Do not Mirror the bitmap in this call */
        public const uint CAPTUREBLT = 0x40000000;     /* Include layered windows */

        // StretchBlt() Modes
        public const int BLACKONWHITE = 1;
        public const int WHITEONBLACK = 2;
        public const int COLORONCOLOR = 3;
        public const int HALFTONE = 4;
        public const int MAXSTRETCHBLTMODE = 4;

        public const int HeapCompatibilityInformation = 0;
        public const uint HEAP_NO_SERIALIZE = 0x00000001;
        public const uint HEAP_GROWABLE = 0x00000002;
        public const uint HEAP_GENERATE_EXCEPTIONS = 0x00000004;
        public const uint HEAP_ZERO_MEMORY = 0x00000008;
        public const uint HEAP_REALLOC_IN_PLACE_ONLY = 0x00000010;
        public const uint HEAP_TAIL_CHECKING_ENABLED = 0x00000020;
        public const uint HEAP_FREE_CHECKING_ENABLED = 0x00000040;
        public const uint HEAP_DISABLE_COALESCE_ON_FREE = 0x00000080;
        public const uint HEAP_CREATE_ALIGN_16 = 0x00010000;
        public const uint HEAP_CREATE_ENABLE_TRACING = 0x00020000;
        public const uint HEAP_MAXIMUM_TAG = 0x0FFF;
        public const uint HEAP_PSEUDO_TAG_FLAG = 0x8000;
        public const uint HEAP_TAG_SHIFT = 18;

        public const int SM_TABLETPC = 86;

        public const uint MONITOR_DEFAULTTONULL = 0x00000000;
        public const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        public const uint WTD_UI_ALL = 1;
        public const uint WTD_UI_NONE = 2;
        public const uint WTD_UI_NOBAD = 3;
        public const uint WTD_UI_NOGOOD = 4;

        public const uint WTD_REVOKE_NONE = 0;
        public const uint WTD_REVOKE_WHOLECHAIN = 1;

        public const uint WTD_CHOICE_FILE = 1;
        public const uint WTD_CHOICE_CATALOG = 2;
        public const uint WTD_CHOICE_BLOB = 3;
        public const uint WTD_CHOICE_SIGNER = 4;
        public const uint WTD_CHOICE_CERT = 5;

        public const uint WTD_STATEACTION_IGNORE = 0;
        public const uint WTD_STATEACTION_VERIFY = 1;
        public const uint WTD_STATEACTION_CLOSE = 2;
        public const uint WTD_STATEACTION_AUTO_CACHE = 3;
        public const uint WTD_STATEACTION_AUTO_CACHE_FLUSH = 4;

        public const uint WTD_PROV_FLAGS_MASK = 0x0000FFFF;
        public const uint WTD_USE_IE4_TRUST_FLAG = 0x00000001;
        public const uint WTD_NO_IE4_CHAIN_FLAG = 0x00000002;
        public const uint WTD_NO_POLICY_USAGE_FLAG = 0x00000004;
        public const uint WTD_REVOCATION_CHECK_NONE = 0x00000010;
        public const uint WTD_REVOCATION_CHECK_END_CERT = 0x00000020;
        public const uint WTD_REVOCATION_CHECK_CHAIN = 0x00000040;
        public const uint WTD_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 0x00000080;
        public const uint WTD_SAFER_FLAG = 0x00000100;
        public const uint WTD_HASH_ONLY_FLAG = 0x00000200;
        public const uint WTD_USE_DEFAULT_OSVER_CHECK = 0x00000400;
        public const uint WTD_LIFETIME_SIGNING_FLAG = 0x00000800;
        public const uint WTD_CACHE_ONLY_URL_RETRIEVAL = 0x00001000;

        public static Guid WINTRUST_ACTION_GENERIC_VERIFY_V2
        {
            get
            {
                return new Guid(0xaac56b, 0xcd44, 0x11d0, 0x8c, 0xc2, 0x0, 0xc0, 0x4f, 0xc2, 0x95, 0xee);
            }
        }

        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;
        public const uint FILE_SHARE_DELETE = 0x00000004;

        public const uint FILE_READ_DATA = 0x0001;
        public const uint FILE_LIST_DIRECTORY = 0x0001;
        public const uint FILE_WRITE_DATA = 0x0002;
        public const uint FILE_ADD_FILE = 0x0002;
        public const uint FILE_APPEND_DATA = 0x0004;
        public const uint FILE_ADD_SUBDIRECTORY = 0x0004;
        public const uint FILE_CREATE_PIPE_INSTANCE = 0x0004;

        public const uint FILE_READ_EA = 0x0008;
        public const uint FILE_WRITE_EA = 0x0010;
        public const uint FILE_EXECUTE = 0x0020;
        public const uint FILE_TRAVERSE = 0x0020;
        public const uint FILE_DELETE_CHILD = 0x0040;
        public const uint FILE_READ_ATTRIBUTES = 0x0080;
        public const uint FILE_WRITE_ATTRIBUTES = 0x0100;
        public const uint FILE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x1FF);
        public const uint FILE_GENERIC_READ = (STANDARD_RIGHTS_READ | FILE_READ_DATA | FILE_READ_ATTRIBUTES | FILE_READ_EA | SYNCHRONIZE);
        public const uint FILE_GENERIC_WRITE = (STANDARD_RIGHTS_WRITE | FILE_WRITE_DATA | FILE_WRITE_ATTRIBUTES | FILE_WRITE_EA | FILE_APPEND_DATA | SYNCHRONIZE);
        public const uint FILE_GENERIC_EXECUTE = (STANDARD_RIGHTS_EXECUTE | FILE_READ_ATTRIBUTES | FILE_EXECUTE | SYNCHRONIZE);

        public const uint READ_CONTROL = 0x00020000;
        public const uint SYNCHRONIZE = 0x00100000;
        public const uint STANDARD_RIGHTS_READ = READ_CONTROL;
        public const uint STANDARD_RIGHTS_WRITE = READ_CONTROL;
        public const uint STANDARD_RIGHTS_EXECUTE = READ_CONTROL;
        public const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;

        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint GENERIC_EXECUTE = 0x20000000;

        public const uint CREATE_NEW = 1;
        public const uint CREATE_ALWAYS = 2;
        public const uint OPEN_EXISTING = 3;
        public const uint OPEN_ALWAYS = 4;
        public const uint TRUNCATE_EXISTING = 5;

        public const uint FILE_ATTRIBUTE_READONLY = 0x00000001;
        public const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;
        public const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        public const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
        public const uint FILE_ATTRIBUTE_DEVICE = 0x00000040;
        public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        public const uint FILE_ATTRIBUTE_TEMPORARY = 0x00000100;
        public const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200;
        public const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
        public const uint FILE_ATTRIBUTE_COMPRESSED = 0x00000800;
        public const uint FILE_ATTRIBUTE_OFFLINE = 0x00001000;
        public const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
        public const uint FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;

        public const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        public const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
        public const uint FILE_FLAG_RANDOM_ACCESS = 0x10000000;
        public const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;
        public const uint FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;
        public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        public const uint FILE_FLAG_POSIX_SEMANTICS = 0x01000000;
        public const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
        public const uint FILE_FLAG_OPEN_NO_RECALL = 0x00100000;
        public const uint FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;

        public const uint FILE_BEGIN = 0;
        public const uint FILE_CURRENT = 1;
        public const uint FILE_END = 2;

        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const uint HANDLE_FLAG_INHERIT = 0x1;
        public const uint HANDLE_FLAG_PROTECT_FROM_CLOSE = 0x2;

        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RESERVE = 0x2000;
        public const uint MEM_DECOMMIT = 0x4000;
        public const uint MEM_RELEASE = 0x8000;
        public const uint MEM_RESET = 0x80000;
        public const uint MEM_TOP_DOWN = 0x100000;
        public const uint MEM_PHYSICAL = 0x400000;

        public const uint PAGE_NOACCESS = 0x01;
        public const uint PAGE_READONLY = 0x02;
        public const uint PAGE_READWRITE = 0x04;
        public const uint PAGE_WRITECOPY = 0x08;
        public const uint PAGE_EXECUTE = 0x10;
        public const uint PAGE_EXECUTE_READ = 0x20;
        public const uint PAGE_EXECUTE_READWRITE = 0x40;
        public const uint PAGE_EXECUTE_WRITECOPY = 0x80;
        public const uint PAGE_GUARD = 0x100;
        public const uint PAGE_NOCACHE = 0x200;
        public const uint PAGE_WRITECOMBINE = 0x400;

        public const uint SEC_IMAGE = 0x1000000;
        public const uint SEC_RESERVE = 0x4000000;
        public const uint SEC_COMMIT = 0x8000000;
        public const uint SEC_NOCACHE = 0x10000000;

        public const uint SECTION_QUERY = 0x0001;
        public const uint SECTION_MAP_WRITE = 0x0002;
        public const uint SECTION_MAP_READ = 0x0004;
        public const uint SECTION_MAP_EXECUTE_EXPLICIT = 0x0020;

        public const uint FILE_MAP_COPY = SECTION_QUERY;
        public const uint FILE_MAP_WRITE = SECTION_MAP_WRITE;
        public const uint FILE_MAP_READ = SECTION_MAP_READ;
        public const uint FILE_MAP_EXECUTE = SECTION_MAP_EXECUTE_EXPLICIT;

        public const uint GMEM_FIXED = 0x0000;
        public const uint GMEM_MOVEABLE = 0x0002;
        public const uint GMEM_ZEROINIT = 0x0040;
        public const uint GHND = 0x0042;
        public const uint GPTR = 0x0040;

        public const uint DIB_RGB_COLORS = 0; /* color table in RGBs */
        public const uint DIB_PAL_COLORS = 1; /* color table in palette indices */

        public const uint BI_RGB = 0;
        public const uint BI_RLE8 = 1;
        public const uint BI_RLE4 = 2;
        public const uint BI_BITFIELDS = 3;
        public const uint BI_JPEG = 4;
        public const uint BI_PNG = 5;

        public const uint DT_TOP = 0x00000000;
        public const uint DT_LEFT = 0x00000000;
        public const uint DT_CENTER = 0x00000001;
        public const uint DT_RIGHT = 0x00000002;
        public const uint DT_VCENTER = 0x00000004;
        public const uint DT_BOTTOM = 0x00000008;
        public const uint DT_WORDBREAK = 0x00000010;
        public const uint DT_SINGLELINE = 0x00000020;
        public const uint DT_EXPANDTABS = 0x00000040;
        public const uint DT_TABSTOP = 0x00000080;
        public const uint DT_NOCLIP = 0x00000100;
        public const uint DT_EXTERNALLEADING = 0x00000200;
        public const uint DT_CALCRECT = 0x00000400;
        public const uint DT_NOPREFIX = 0x00000800;
        public const uint DT_public = 0x00001000;

        public const uint DT_EDITCONTROL = 0x00002000;
        public const uint DT_PATH_ELLIPSIS = 0x00004000;
        public const uint DT_END_ELLIPSIS = 0x00008000;
        public const uint DT_MODIFYSTRING = 0x00010000;
        public const uint DT_RTLREADING = 0x00020000;
        public const uint DT_WORD_ELLIPSIS = 0x00040000;
        public const uint DT_NOFULLWIDTHCHARBREAK = 0x00080000;
        public const uint DT_HIDEPREFIX = 0x00100000;
        public const uint DT_PREFIXONLY = 0x00200000;

        public const uint FW_DONTCARE = 0;
        public const uint FW_THIN = 100;
        public const uint FW_EXTRALIGHT = 200;
        public const uint FW_LIGHT = 300;
        public const uint FW_NORMAL = 400;
        public const uint FW_MEDIUM = 500;
        public const uint FW_SEMIBOLD = 600;
        public const uint FW_BOLD = 700;
        public const uint FW_EXTRABOLD = 800;
        public const uint FW_HEAVY = 900;

        public const uint OUT_DEFAULT_PRECIS = 0;
        public const uint OUT_STRING_PRECIS = 1;
        public const uint OUT_CHARACTER_PRECIS = 2;
        public const uint OUT_STROKE_PRECIS = 3;
        public const uint OUT_TT_PRECIS = 4;
        public const uint OUT_DEVICE_PRECIS = 5;
        public const uint OUT_RASTER_PRECIS = 6;
        public const uint OUT_TT_ONLY_PRECIS = 7;
        public const uint OUT_OUTLINE_PRECIS = 8;
        public const uint OUT_SCREEN_OUTLINE_PRECIS = 9;
        public const uint OUT_PS_ONLY_PRECIS = 10;

        public const uint CLIP_DEFAULT_PRECIS = 0;
        public const uint CLIP_CHARACTER_PRECIS = 1;
        public const uint CLIP_STROKE_PRECIS = 2;
        public const uint CLIP_MASK = 0xf;
        public const uint CLIP_LH_ANGLES = (1 << 4);
        public const uint CLIP_TT_ALWAYS = (2 << 4);
        public const uint CLIP_EMBEDDED = (8 << 4);

        public const uint DEFAULT_QUALITY = 0;
        public const uint DRAFT_QUALITY = 1;
        public const uint PROOF_QUALITY = 2;
        public const uint NONANTIALIASED_QUALITY = 3;
        public const uint ANTIALIASED_QUALITY = 4;

        public const uint CLEARTYPE_QUALITY = 5;

        public const uint CLEARTYPE_NATURAL_QUALITY = 6;

        public const uint DEFAULT_PITCH = 0;
        public const uint FIXED_PITCH = 1;
        public const uint VARIABLE_PITCH = 2;
        public const uint MONO_FONT = 8;

        public const uint FF_DONTCARE = (0 << 4);
        public const uint FF_ROMAN = (1 << 4);
        public const uint FF_SWISS = (2 << 4);
        public const uint FF_MODERN = (3 << 4);
        public const uint FF_SCRIPT = (4 << 4);
        public const uint FF_DECORATIVE = (5 << 4);

        public const int SB_HORZ = 0;

        public const int S_OK = 0;
        public const int S_FALSE = 1;
        public const int E_NOTIMPL = unchecked((int)0x80004001);
    }  
}
