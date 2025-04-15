<script lang="ts">
	import { goto } from "$app/navigation";
	import { logout } from "$lib/App.svelte";
    import { Button, Overlay, ProgressCircle } from "svelte-ux";



    async function doLogout() {
        await logout();
        goto("/");
    }
</script>

<Button color="danger" variant="fill-outline" onclick={async () => doLogout()}>Déconnexion</Button>