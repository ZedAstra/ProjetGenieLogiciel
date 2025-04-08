import { goto } from "$app/navigation";
import * as jose from "jose";

let appState: {
    jwtString: string,
    userId: string,
    isLoggedIn: boolean
} = $state({
    jwtString: "",
    userId: "",
    isLoggedIn: false
});

export function getAppState() {
    if(!appState.isLoggedIn && appState.jwtString == "") {
        loadState();
    }
    return appState!;
}


export function getJwtToken(): string | null  {
    return getAppState().jwtString;
}


export function setJwtToken(value: string) {
    if(jwtIsValid(value)) {
        getAppState().jwtString = value;
    }
}
export function jwtIsValid(token: string): boolean {
    const decoded = jose.decodeJwt(token);
    // Check if the token expired
    if(decoded.exp && decoded.exp < Date.now() / 1000) {
        return false;
    } else {
        localStorage.setItem('jwt', token);
        appState = {
            jwtString: token,
            userId: decoded.sub!,
            isLoggedIn: true
        };
        return true;
    }
}
export function getUserId() {
    return getAppState().userId;
}
export function getUserRole(): "Admin" | "Chef" | "Partenaire" | "Artisan" | "Ouvrier" | "None" {
    var token = getJwtToken();
    if(token == null) return "None";
    const decoded = jose.decodeJwt(token);
    if(decoded.role) {
        if((decoded.role as string).includes("Admin")) return "Admin";
        if((decoded.role as string).includes("Chef")) return "Chef";
        if((decoded.role as string).includes("Partenaire")) return "Partenaire";
        if((decoded.role as string).includes("Artisan")) return "Artisan";
        if((decoded.role as string).includes("Ouvrier")) return "Ouvrier";
    }
    return "None";
}

export function getUserName(): string {
    var token = getJwtToken();
    if(token == null) return "";
    const decoded = jose.decodeJwt(token);
    if(decoded.name) {
        return decoded.name as string;
    }
    return "";
}

export function appFetch(endpoint: string, init: RequestInit): Promise<Response> {
    if(init.headers == null) init.headers = {};
    if(endpoint.startsWith("/")) endpoint = endpoint.substring(1);
    // @ts-ignore
    if(endpoint != "auth/login") init.headers["Authorization"] = "Bearer " + getJwtToken();
    return fetch(`https://localhost:7149/${endpoint}`, init);
}

export function loadState() {
    const jwt = localStorage.getItem('jwt');
    if(jwt) {
        jwtIsValid(jwt);
    }
}

export function logout() {
    localStorage.removeItem('jwt');
    appState = {
        jwtString: "",
        userId: "",
        isLoggedIn: false
    };
    goto("/");
}