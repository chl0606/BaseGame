namespace ABManager
{
    public struct Messages
    {
        public const string BUTTON_CLOSE = "Close";

        public const string TITLE_ERROR = "Error";
        public const string TITLE_ERROR_RENAME = "Rename Error";
        public const string TITLE_ERROR_PREFIX = "Prefix Error";

        public const string MESSAGE_ERROR_RENAME = " is not valid for bundle name. Use only characters, numbers, _ and /#-";
        public const string MESSAGE_ERROR_PREFIX = " is not valid. Use only characters, numbers, _ and /#-";
        public const string MESSAGE_ERROR_EXTENSION = "Cannot mark assets and scenes in one AssetBundle.";
        public const string MESSAGE_ERROR_FOLDERASSET = "Cannot removing the asset folders inside.\nRemove a folder's BundleName directly. ";
    }
}
