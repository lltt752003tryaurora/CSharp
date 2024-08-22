using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public enum ErrorCode
    {
        UNHANDLE_ERROR = 0,
        SUCCESSFULL = 1,
        AUTHENTICATE_FAIL = 2,
        DUPPLICATE_TRANSACTION = 3,
        UNKNOW_COMMAND = 4,
        BODY_INVALID = 5,
        NOT_ALLOWED_IP = 6,
        DB_EXCUTE_FAIL = -1,
        LOAD_DATA_FROM_DB_FAIL = -1001,
        INSERT_DATA_TO_DB_FAIL = -1002,
        UPDATE_DATA_TO_DB_FAIL = -1003,
        PROMOTION_SCORE_NOT_ENOUGH = -1004,
        BALANCE_NOT_EXISTS = -1005,
        WRONG_DATASIGN = 2106,
        NOT_HAVE_PERMISSION_CALL_METHOD = 2016,
        CANCEL_DELETE = 2017
    }
    public class EC
    {
        public const string UNHANDLE_ERROR = "0";
        public const string SUCCESSFULL = "1";
        public const string AUTHENTICATE_FAIL = "2";
        public const string DUPPLICATE_TRANSACTION = "3";
        public const string UNKNOW_COMMAND = "4";
        public const string BODY_INVALID = "5";
        public const string NOT_ALLOWED_IP = "6";
        public const string DB_EXCUTE_FAIL = "-1";
        public const string LOAD_DATA_FROM_SESSION_FAIL = "-1000";
        public const string LOAD_DATA_FROM_DB_FAIL = "-1001";
        public const string INSERT_DATA_TO_DB_FAIL = "-1002";
        public const string UPDATE_DATA_TO_DB_FAIL = "-1003";
        public const string BALANCE_NOT_EXISTS = "-1005";
        public const string LOGIN_FAIL = "-1006";
        public const string REQUIRE_LOGIN = "-1007";
    }

    public class APIEC
    {
        public const string UNHANDLE_ERROR = "0";
        public const string SUCCESSFULL = "1";
        public const string AUTHENTICATE_FAIL = "2";
        public const string DUPPLICATE_TRANSACTION = "3";
        public const string UNKNOW_COMMAND = "4";
        public const string HEADER_INVALID = "5";
        public const string BODY_INVALID = "6";
        public const string NOT_ALLOWED_IP = "7";
        public const string NOT_HAVE_PERMISSION_CALL_METHOD = "8";
        public const string CHECKSUM_INVALID = "9";
        public const string DB_EXCUTE_FAIL = "-1";
        public const string LOAD_DATA_FROM_SESSION_FAIL = "-1000";
        public const string LOAD_DATA_FROM_DB_FAIL = "-1001";
        public const string INSERT_DATA_TO_DB_FAIL = "-1002";
        public const string UPDATE_DATA_TO_DB_FAIL = "-1003";
        public const string NOT_FOUND = "-1004";

        public const string USER_EXISTS = "100";
        public const string USER_NOT_FOUND = "101";
        public const string USER_EMAIL_INVALID = "102";
        public const string USER_PHONE_INVALID = "103";
        public const string USER_PASS_NOT_MEET_REQUIREMENT = "104";
        public const string SESSION_EXPIRE = "105";
        public const string USER_PASS_NOT_MATCH = "106";
        public const string USER_NEW_PASS_NOT_MATCH = "108";

        public const string NOT_ENOUGH_QUESTION = "200";

    }
    public class ExamStatus
    {
        public const long HIDE = 1;
        public const long NOW = 2;
        public const long CONTINUES = 3;
        public const long OPENINGSOON = 4;
        public const long EXPIRES = 5;
        public const long RESULT = 6;
        public const long RESUBMIT = 7;
    }
}
