<script lang="ts">
    import { LoaderCircle } from "lucide-svelte"
    import { Button } from "$lib/components/ui/button/index.js";
    import * as Card from "$lib/components/ui/card/index.js";
    import { Input } from "$lib/components/ui/input/index.js";
    import { Label } from "$lib/components/ui/label/index.js";
	import { appFetch, setJwtToken } from "$lib/app.svelte";
	import { goto } from "$app/navigation";

    let loading = $state(false);
    let userLogin = $state("");
    let password = $state("");

    async function login() {
        loading = true;
        const form = new FormData();
        form.append("login", userLogin);
        form.append("password", password);
        try {
            const result = await appFetch("auth/login", {
                method: "POST",
                body: form
            });
            loading = false;
            if(result?.ok) {
                setJwtToken(await result.text());
                goto("/app");
            } else {
                if(result?.status === 401) {
                    alert("Identifiants incorrects");
                } else {
                    alert("An error occurred");
                }
            }
        }
        catch(e) {
            loading = false;
            alert("An error occurred");
        }
    }
</script>

<div class="w-screen h-screen flex items-center justify-center">
    <Card.Root class="w-[350px]">
        <Card.Header>
            <Card.Title>Se connecter</Card.Title>
            <Card.Description>Entrez vos identifiants</Card.Description>
        </Card.Header>
        <Card.Content>
          <form>
            <div class="grid w-full items-center gap-4">
              <div class="flex flex-col space-y-1.5">
                <Label for="login">Identifiant</Label>
                <Input id="login" placeholder="Identifiant (nom ou email)" disabled={loading} bind:value={userLogin}/>
              </div>
              <div class="flex flex-col space-y-1.5">
                <Label for="password">Mot de passe</Label>
                <Input id="password" type="password" placeholder="Mot de passe" disabled={loading} bind:value={password}/>
              </div>
              <Button class="w-full" disabled={loading} onclick={login}>
                Se connecter
                {#if loading}
                    <LoaderCircle class="animate-spin" />
                {/if}
              </Button>
            </div>
          </form>
        </Card.Content>
        <Card.Footer class="flex justify-between">
        </Card.Footer>
    </Card.Root>
</div>