import { logout } from "./app.svelte";

export function injectPlatform() {
    if (typeof (window as any).platform === 'undefined') {
        (window as any).platform = {
            getServerUrl: () => {
                return 'https://localhost:7149';
            }
        }
    }
    if(typeof(window as any).__app__ === 'undefined') {
        (window as any).__app__ = {};
        (window as any).__app__.logout = logout;
    }
}