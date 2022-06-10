using PBL3.DTO;

namespace PBL3.Service {
    public interface IEmailService {
        Task SendEmail(EmailDto email);
    }
}
