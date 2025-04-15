<script lang="ts">
    import { Button, Card, Field, Form, Input, TextField } from 'svelte-ux';
    import { login } from '$lib/App.svelte';

    let loginValue: string = $state("");
    let password: string = $state("");

    let isLoading = $state(false);

    async function appLogin() {
        if(!loginValue || !password) {
            alert('Please fill in all fields');
            return;
        }
        isLoading = true;
        var data = await login(loginValue, password);
        isLoading = false;
        if (data) {
            // Redirect to the app
            window.location.href = '/app';
        } else {
            // Handle login error
            alert('Login failed');
        }
    }
</script>

<div class="h-[calc(100vh-64px)] flex justify-center items-center">
    {#if isLoading}
        <div class="absolute inset-0 bg-gray-200 opacity-50 flex justify-center items-center z-50">
            <div class="size-4 animate-spin bg-white"></div>
        </div>
    {/if}
    <Card class="p-4 gap-2">
        <TextField label="Identifiant" bind:value={loginValue} />
        <TextField type="password" label="Mot de passe" bind:value={password}/>
        <Button variant="fill" color="primary" type="submit" class="w-full" onclick={appLogin}>Se connecter</Button>
    </Card>
</div>