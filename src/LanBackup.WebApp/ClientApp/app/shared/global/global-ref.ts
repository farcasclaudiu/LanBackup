export interface MyGlobal {
  clientSettings: {
    admin_email: string,
    instrumentationEnabled: boolean,
    instrumentationKey: string
  };
}

export abstract class GlobalRef {
  abstract get nativeGlobal(): MyGlobal;
}

export class BrowserGlobalRef extends GlobalRef {
  get nativeGlobal(): MyGlobal { return window as MyGlobal; }
}

export class NodeGlobalRef extends GlobalRef {
  get nativeGlobal(): MyGlobal { return global as MyGlobal; }
}