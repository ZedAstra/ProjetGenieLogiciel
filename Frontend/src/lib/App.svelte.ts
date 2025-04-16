import createClient from "openapi-fetch";
import type { components, paths } from "./v1";
import { goto } from "$app/navigation";


let jwtToken: string = $state(localStorage.getItem("jwtToken") || "null");

let chantier: number = $state(JSON.parse(localStorage.getItem("chantier") || "0"));
export function setChantier(id: number) {
	chantier = id;
	localStorage.setItem("chantier", JSON.stringify(id));
}
export function getChantier() {
	if (chantier) {
		chantier = JSON.parse(localStorage.getItem("chantier") || "0");
	}
	return chantier;
}

let client = createClient<paths>({
	baseUrl: "https://localhost:7149",
})
let me = $state<components["schemas"]["SafeUtilisateur"] | null>(JSON.parse(localStorage.getItem("me") || "null"));
export async function login(username: string, password: string): Promise<string | null> {
	const { data, error } = await client.POST("/auth/login", {
		body: {
			login: username,
			password: password,
		},
		bodySerializer: formBodySerializer,
	});
	if (data) {
		jwtToken = data;
		localStorage.setItem("jwtToken", jwtToken);
		try {
			await defineSelf();
		}
		catch (e) {
			console.error(e);
		}
		return jwtToken;
	} else {
		return null;
	}
}

async function defineSelf() {
	const { data , error } = await client.GET("/auth/whoami", {
		params: {
			// @ts-ignores
			header: {
				Authorization: getBearerToken(),
			}
		}
	})
	if(data) {
		me = data;
		localStorage.setItem("me", JSON.stringify(me));
	}
	if (error) {
		console.error(error);
		return null;
	}
}
export function getMe() {
	if (me) {
		me = JSON.parse(localStorage.getItem("me") || "null");
	}
	return me;
}

export function getBearerToken() {
	if (jwtToken === "null") {
		jwtToken = localStorage.getItem("jwtToken") || "";
	}
	return "Bearer " + jwtToken;
}

export function isLoggedIn() {
	return jwtToken !== "null" && jwtToken !== "" && jwtToken !== null;
}

export async function logout() {
	jwtToken = "";
	localStorage.removeItem("jwtToken");
	me = null;
	localStorage.removeItem("me");
	goto("/");
}

export function getClient() {
    return client;
}


export async function setJwtToken(token: string) {
  jwtToken = token;
  localStorage.setItem("jwtToken", token);
  let idk: components["schemas"]["CompactChantier"]
}

export function formBodySerializer(body: any) {
	const formData = new FormData();
	for (const name in body) {
	  formData.append(name, body[name as keyof typeof body]);
	}
	return formData;
}

export function isAnyOf(target: any, ...args: any[]) {
	return args.some((arg) => {
		if (typeof arg === "string") {
			return target === arg;
		} else if (typeof arg === "object") {
			return Object.keys(arg).every((key) => {
				return target[key] === arg[key];
			});
		}
		return false;
	});
}