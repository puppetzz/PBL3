namespace PBL3.DTO {
    public class EmailDto {
        private string _content;
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Type { get; set; }
        public string Content {
            get {
                string title = "";
                string verifyContent = "Thanks for starting the new Shop guitar account creation process. We want to make sure it's really you. Please enter the following verification code when prompted. If you don’t want to create an account, you can ignore this message.";
                if (Type == "Verify") {
                    title = "Verify your account";
                }
                if (Type == "Reset") {
                    title = "Reset your Password";
                    verifyContent = "Your request to forget your password has been passed. Please enter the following verification code when prompted to reset your password.";
                }
                return $"    <div id=':nk' class='a3s aiL msg3970125449445742223'><u></u>" +
                $"        <div width='100%' style='margin:0;background-color:#f0f2f3'>" +
                $"        <div style='margin:auto;max-width:600px;padding-top:50px' class='m_3970125449445742223email-container'>" +
                $"            <table role='presentation' cellspacing='0' cellpadding='0' width='100%' align='center' id='m_3970125449445742223logoContainer' style='background:#252f3d;border-radius:3px 3px 0 0;max-width:600px'>" +
                $"                <tbody><tr>" +
                $"                    <td style='background:#252f3d;border-radius:3px 3px 0 0;padding:20px 0 10px 0;text-align:center; color: white; font-family:Arial, Helvetica, sans-serif; font-size: 2rem'>" +
                $"                        SHOP GUITAR" +
                $"                    </td>" +
                $"                </tr>" +
                $"            </tbody></table>" +
                $"            <table role='presentation' cellspacing='0' cellpadding='0' width='100%' align='center' id='m_3970125449445742223emailBodyContainer' style='border:0px;border-bottom:1px solid #d6d6d6;max-width:600px'>" +
                $"                <tbody><tr>" +
                $"                    <td style='background-color:#fff;color:#444;font-family:\"Amazon Ember\",\"Helvetica Neue\",Roboto,Arial,sans-serif;font-size:14px;line-height:140%;padding:25px 35px'>" +
                $"                        <h1 style='font-size:20px;font-weight:bold;line-height:1.3;margin:0 0 15px 0'>{title}</h1>" +
                $"                        <p style='margin:0;padding:0'>{verifyContent}</p>" +
                $"                        <p style='margin:0;padding:0'></p>" +
                $"                    </td>" +
                $"                </tr>" +
                $"                <tr>" +
                $"                    <td style='background-color:#fff;color:#444;font-family:\"Amazon Ember\",\"Helvetica Neue\",Roboto,Arial,sans-serif;font-size:14px;line-height:140%;padding:25px 35px;padding-top:0;text-align:center'>" +
                $"                        <div style='font-weight:bold;padding-bottom:15px'>Verification code</div>" +
                $"                        <div style='color:#000;font-size:36px;font-weight:bold;padding-bottom:15px'>{_content}</div>" +
                $"                        <div>(This code is valid for 5 minutes)</div>" +
                $"                    </td>" +
                $"                </tr>" +
                $"                <tr>" +
                $"                    <td style='background-color:#fff;border-top:1px solid #e0e0e0;color:#777;font-family:\"Amazon Ember\",\"Helvetica Neue\",Roboto,Arial,sans-serif;font-size:14px;line-height:140%;padding:25px 35px'>" +
                $"                        <p style='margin:0 0 15px 0;padding:0 0 0 0'>Shop Guitar Web Services will never email you and ask you to disclose or verify your password, credit card, or banking account number.</p>" +
                $"                    </td>" +
                $"                </tr>" +
                $"            </tbody></table>" +
                $"        </div>" +
                $"    </div>";
            }
            set {
                _content = value;
            }
        }
    }
}
