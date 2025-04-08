import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
	return twMerge(clsx(inputs));
}

export function sGoto(url: string) {
	if(url.startsWith("/")) url = url.substring(1);
	window.location.href = `${window.location.origin}/${url}`;
}
