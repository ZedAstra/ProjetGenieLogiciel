<script lang="ts">
    import { Button, Card, Dialog, Field, Form, Input, Overlay, ProgressCircle, SelectField, TextField, type MenuOption } from 'svelte-ux';
    import { formBodySerializer, getBearerToken, getClient, getMe, login, setChantier } from '$lib/App.svelte';
	import type { components } from '$lib/v1';
	import { goto } from '$app/navigation';
	import { get } from 'svelte/store';

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
            await fetchProjects();
            currentStep = 1;
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

    let newChantierDialogOpen = $state(false);
    let newChantier: components["schemas"]["CreateChantierForm"] = $state({
        nom: "",
        details: "",
        dateDebut: new Date().toISOString(),
        status: "EnAttente",
        membres: [getMe()?.id!],
    });
    async function createChantier() {
        if (!newChantier.nom) {
            alert("Veuillez entrer un nom pour le chantier");
            return;
        }
        if (!newChantier.details) {
            alert("Veuillez entrer des détails pour le chantier");
            return;
        }
        if (!newChantier.dateDebut) {
            alert("Veuillez entrer une date de début pour le chantier");
            return;
        }
        isLoading = true;
        const { data, error } = await getClient().POST("/app/projects/create", {
            params: {
                // @ts-ignore
                header: {
                    Authorization: getBearerToken(),
                },
            },
            body: newChantier,
            bodySerializer: formBodySerializer,
        });
        if (error) {
            console.error(error);
            isLoading = false;
            return;
        }
        if (data) {
            projects.push(data);
            options.push({
                label: data.nom!,
                value: data,
            });
            newChantierDialogOpen = false;
        }
        isLoading = false;
    }
    function clearNewChantier() {
        newChantier = {
            nom: "",
            details: "",
            dateDebut: new Date().toISOString(),
            status: "EnAttente",
            membres: [getMe()?.id!],
        };
    }
</script>

<div class="h-[calc(100vh-64px)] flex justify-center items-center">
    {#if isLoading}
        <Overlay center>
            <ProgressCircle/>
        </Overlay>
    {/if}
    {#if currentStep == 0}
    <Card class="p-4 gap-2 w-96" title="Se connecter">
        <TextField label="Identifiant" bind:value={loginValue} />
        <TextField type="password" label="Mot de passe" bind:value={password}/>
        <Button variant="fill" color="primary" type="submit" class="w-full" onclick={appLogin}>Se connecter</Button>
    </Card>
    {:else if currentStep == 1}
    <Card class="p-4 gap-2 w-96" title="Selectionner un chantier">
        <Dialog bind:open={newChantierDialogOpen}>
            <div slot="title">Créer un nouveau chantier</div>
            <TextField label="Nom" bind:value={newChantier.nom} />
            <TextField label="Détails" bind:value={newChantier.details} />
            <Input type="date" label="Date de début" bind:value={newChantier.dateDebut} />
            <SelectField label="Statut" bind:value={newChantier.status} options={[
                { label: "En attente", value: "EnAttente" },
                { label: "En cours", value: "EnCours" },
                { label: "Terminé", value: "Termine" },
            ]} />
            <Button variant="fill" color="primary" class="mt-2" onclick={createChantier}>Créer</Button>
        </Dialog>
        <div class="flex flex-row">
            <SelectField {options} bind:value={selected} label="Chantier"/>
            {#if getMe()?.userRole == "Admin" || getMe()?.userRole == "Chef"}
            <Button variant="fill" color="primary" class="ml-2" onclick={() => {
                newChantierDialogOpen = true;
                clearNewChantier();
            }}>Créer</Button>
            {/if}
        </div>
        <Button variant="fill" color="primary" type="submit" class="w-full" onclick={async () => {
            if (selected) {
                setChantier(selected.id!);
                goto("/app")
            }
        }}>Se connecter</Button>
    </Card>
    {/if}
</div>