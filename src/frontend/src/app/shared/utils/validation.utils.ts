export class ValidationUtils {
  private static readonly EMAIL_REGEX = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  private static readonly URL_REGEX = /^(https?:\/\/)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*\/?$/;

  static isValidEmail(email: string): boolean {
    if (!email) return false;
    return this.EMAIL_REGEX.test(email.trim());
  }

  static isValidUrl(url: string): boolean {
    if (!url) return false;
    return this.URL_REGEX.test(url.trim());
  }

  static isNotEmpty(value: string | null | undefined): boolean {
    return value !== null && value !== undefined && value.trim().length > 0;
  }
}
