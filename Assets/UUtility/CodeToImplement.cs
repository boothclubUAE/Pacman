using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using UnityEngine;

public class CodeToImplement : MonoBehaviour
{
    public static CodeToImplement _instance;

    private void Awake()
    {
        _instance  = this;
    }

    //------------------------------------------------------------------------------------------------|
    //Add Code here that needs to be added to the Tool.
    //Describe the the intent for the code and any other notes like "Needs to be Improved/Revised" Etc.
    //v---v---v---v---v---v---v---v---v---v---v---v---v---v---v---v---v---v---v---v---v---v---v---v---v

    static bool IsUAEPhoneNumber(string number)
    {
        // Matches "05" followed by 8 digits or "9715" followed by 9 digits
        var regex = new Regex(@"^(05\d{8}|9715\d{8})$");
        return regex.IsMatch(number);
    }

    //use this as new valid email checker, refractured it
    public static bool IsValidEmail(string emailID)
    {
        try
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            if (!Regex.IsMatch(emailID, pattern))
                return false;

            var addr = new MailAddress(emailID);
            return addr.Address == emailID;
        }
        catch
        {
            return false;
        }
    }
}
