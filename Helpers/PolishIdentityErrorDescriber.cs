using Microsoft.AspNetCore.Identity;

public class PolishIdentityErrorDescriber : IdentityErrorDescriber
{
	public override IdentityError PasswordRequiresDigit()
		=> new() { Code = nameof(PasswordRequiresDigit), Description = "Hasło musi zawierać co najmniej jedną cyfrę ('0'-'9')." };

	public override IdentityError PasswordRequiresNonAlphanumeric()
		=> new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Hasło musi zawierać znak specjalny." };

	public override IdentityError PasswordRequiresUpper()
		=> new() { Code = nameof(PasswordRequiresUpper), Description = "Hasło musi zawierać co najmniej jedną wielką literę ('A'-'Z')." };

	public override IdentityError PasswordTooShort(int length)
		=> new() { Code = nameof(PasswordTooShort), Description = $"Hasło musi mieć co najmniej {length} znaków." };

	public override IdentityError DuplicateUserName(string userName)
		=> new() { Code = nameof(DuplicateUserName), Description = $"Adres e-mail '{userName}' jest już zajęty." };
}
