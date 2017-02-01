export class SignInResult {
  succeeded: boolean;
  isLockedOut: boolean;
  isNotAllowed: boolean;
  requiresTwoFactor: boolean;

  constructor(data) {
    Object.assign(this, data);
  }
}