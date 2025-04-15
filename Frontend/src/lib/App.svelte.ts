import createClient from "openapi-fetch";
import type { components, paths } from "./v1";


let jwtToken: string = $state("");

let client = createClient<paths>({
	baseUrl: "https://localhost:7149",
})
export async function login(username: string, password: string): Promise<string | null> {
	var response = await client.POST("/auth/login", {
		body: {
			login: username,
			password: password,
		},
		bodySerializer: formBodySerializer,
	});
	if (response.data) {
		jwtToken = response.data;
		return jwtToken;
	} else {
		return null;
	}
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
