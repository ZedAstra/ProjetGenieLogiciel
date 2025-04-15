<script lang="ts">
    import { Button, Card, Field, Form, Input, Overlay, ProgressCircle, SelectField, TextField, type MenuOption } from 'svelte-ux';
    import { getBearerToken, getClient, login, setChantier } from '$lib/App.svelte';
	import type { components } from '$lib/v1';
	import { goto } from '$app/navigation';

    let loginValue: string = $state("");
    let password: string = $state("");

    let isLoading = $state(false);

    let currentStep = $state(0);

    async function appLogin() {
        if(!loginValue || !password) {
            alert('Please fill in all fields');
            return;
        }
        isLoading = true;
        var data = await login(loginValue, password);
        if (data) {
            currentStep = 1;
            await fetchProjects();
        } else {
            // Handle login error
            alert('Login failed');
            isLoading = false;
        }
    }
    let projects: components["schemas"]["CompactChantier"][] = $state([]);
    let options: MenuOption<components["schemas"]["CompactChantier"]>[] = $state([]);
    let selected: components["schemas"]["CompactChantier"] | null = $state(null);
    async function fetchProjects() {
        const { data, error } = await getClient().GET("/app/projects", {
            params: {
                // @ts-ignore
                header: {
                    Authorization: getBearerToken(),
                },
            },
        });
        if (error) {
            console.error(error);
            isLoading = false;
            return;
        }
        if (data) {
            projects = data;
            options = projects.map((project) => {
                return {
                    label: project.nom!,
                    value: project,
                };
            });
        }
        isLoading = false;
    }
</script>

<div class="h-[calc(100vh-64px)] flex justify-center items-center">
    {#if isLoading}
        <Overlay center>
            <ProgressCircle/>
        </Overlay>
    {/if}
    {#if currentStep == 0}
    <Card class="p-4 gap-2 w-96">
        <TextField label="Identifiant" bind:value={loginValue} />
        <TextField type="password" label="Mot de passe" bind:value={password}/>
        <Button variant="fill" color="primary" type="submit" class="w-full" onclick={appLogin}>Se connecter</Button>
    </Card>
    {:else if currentStep == 1}
    <Card class="p-4 gap-2 w-96">
        <SelectField {options} bind:value={selected} label="Chantier"/>
        <Button variant="fill" color="primary" type="submit" class="w-full" onclick={async () => {
            if (selected) {
                setChantier(selected.id!);
                goto("/app")
            }
        }}>Se connecter</Button>
    </Card>
    {/if}
</div>