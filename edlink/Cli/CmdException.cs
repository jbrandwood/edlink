using System;


namespace Edlink {


    public enum CmdExceptionType {
        UnknownCmd,
        UnsupportedCmd,
        Mode
    }

    internal class CmdException : Exception {

        string message = "";

        public CmdException(CmdExceptionType type) {
            InitMsg(type, "");
        }

        public CmdException(CmdExceptionType type, string msg) {

            InitMsg(type, msg);
        }

        public CmdException(string message) {
            this.message = message;
        }

        public override string Message {
            get { return message; }
        }


        void InitMsg(CmdExceptionType type, string msg) {

            switch (type) {
                case CmdExceptionType.UnknownCmd:
                    message = "unknown cmd";
                    break;
                case CmdExceptionType.UnsupportedCmd:
                    message = "unsupported cmd";
                    break;
                case CmdExceptionType.Mode:
                    message = "unknown mode '" + msg + "'";
                    break;
                default:
                    message = "unknown cmd error";
                    break;
            }
        }
    }
}
