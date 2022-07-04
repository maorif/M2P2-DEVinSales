namespace DevInSales.Application.Dtos
{
    public class RegisterUserResponse
    {
        public bool Success { get; private set; }

        public List<string> Errors { get; private set; }

        public RegisterUserResponse() {
            this.Errors = new List<string>();
        }

        public RegisterUserResponse(bool success) {
            this.Success = success;
        }

        public void AddErrors(IEnumerable<string> errors) {
            this.Errors.AddRange(errors);
        }
    }
}