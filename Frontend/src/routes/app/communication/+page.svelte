<script lang="ts">
	import { formBodySerializer, getBearerToken, getChantier, getClient, getMe } from "$lib/App.svelte";
	import type { components } from "$lib/v1";
	import { onMount } from "svelte";
	import { Button, Card, DateField, DatePickerField, Dialog, Input, Overlay, ProgressCircle, Tabs, TextField } from "svelte-ux";

    let me = getMe()!;

    let annonces: components["schemas"]["Annonce"][] = $state([]);

    let isLoading = $state(false);
    onMount(async () => refresh())

    async function refresh() {
        isLoading = true;
        const { data, error } = await getClient().GET("/chantier/{chantierId}/communication/news", {
            params: {
                // @ts-ignore
                header: {
                    Authorization: getBearerToken(),
                },
                path: {
                    chantierId: getChantier(),
                },
            },
        });
        if (error) {
            console.error(error);
            return;
        }
        if (data) {
            annonces = data.sort((a, b) => {
                return new Date(b.date!).getTime() - new Date(a.date!).getTime();
            });
        }
        isLoading = false;
    }

    let options: {
        label: string;
        value: string;
    }[] = [
        {
            label: "Annonces",
            value: "annonces",
        },
    ];
    let currentTab = $state("annonces");
    
    // Annonce
    let newAnnonce = $state({
        titre: "",
        description: "",
        date: new Date().toLocaleString(),
        content: "",
    });

    let isCreatingOrEditing = $state(false);
    function createNew() {
        isCreatingOrEditing = true;
        newAnnonce.titre = "";
        newAnnonce.description = "";
        newAnnonce.date = new Date().toLocaleString();
        newAnnonce.content = "";
    }
    async function publish() {
        const { data, error } = await getClient().POST("/chantier/{chantierId}/communication/news/create", {
            params: {
                // @ts-ignore
                header: {
                    Authorization: getBearerToken(),
                },
                path: {
                    chantierId: getChantier()
                }
            },
            body: {
                titre: newAnnonce.titre,
                description: newAnnonce.description,
                date: new Date(newAnnonce.date).toISOString(),
                content: newAnnonce.content
            },
            bodySerializer: formBodySerializer,
        });
        if (error) {
            console.error(error);
            return;
        }
        if (data) {
            annonces.push(data);
            newAnnonce.titre = "";
            newAnnonce.description = "";
            newAnnonce.date = new Date().toLocaleString();
            newAnnonce.content = "";
        }
        isCreatingOrEditing = false;
    }

    let newsViewerOpen = $state(false);
    let currentNews = $state<components["schemas"]["Annonce"] | null>(null);
</script>

{#if isLoading}
    <Overlay center>
        <ProgressCircle />
    </Overlay>
{/if}
<Tabs {options} classes={{content: "p-2"}}
    bind:value={currentTab}>
    <svelte:fragment slot="content">
        {#if currentTab == "annonces"}
            {#if me.userRole == "Admin" || me.userRole == "Chef" || me.userRole == "Partenaire"}
            <Button variant="fill-outline" color="success" onclick={createNew}>Publier une annonce</Button>
            <Dialog bind:open={isCreatingOrEditing} classes={{ dialog: "p-2 w-[calc(100wh-64px)]" }}>
                <div slot="title">Publier</div>
                <Card class="w-[calc(100vw-64px)]">
                    <div class="flex flex-col gap-2">
                        <TextField label="Titre" bind:value={newAnnonce.titre} />
                        <TextField label="Description" bind:value={newAnnonce.description} />
                        <Input label="Date et heure" type="datetime-local" bind:value={newAnnonce.date} />
                        <TextField label="Contenu" bind:value={newAnnonce.content} />
                    </div>
                    <Button variant="fill" color="primary" onclick={publish}>Publier</Button>
                </Card>
                <div slot="actions">
                    <Button variant="fill" color="danger" class="w-full">Annuler</Button>
                </div>
            </Dialog>
            {/if}

            <div class="flex flex-col gap-2">
                <h1 class="text-2xl font-bold">Annonces</h1>
                {#if annonces.length > 0}
                <Dialog bind:open={newsViewerOpen} classes={{ dialog: "p-2 w-[calc(100wh-64px)]" }}>
                    {@html currentNews!.contenu}
                </Dialog>
                <div class="flex flex-col gap-2">
                    {#each annonces as annonce}
                        <button class="p-4 bg-gray-100 rounded-md cursor-pointer" onclick={() => {
                            currentNews = annonce;
                            newsViewerOpen = true;
                        }}>
                            <h2 class="text-xl font-semibold">{annonce.titre}</h2>
                            <p>{annonce.description}</p>
                            <p>{new Date(annonce.date!).toLocaleString()}</p>
                        </button>
                    {/each}
                </div>
                {:else}
                    <p>Aucune annonce disponible.</p>
                {/if}
            </div>
        {/if}
    </svelte:fragment>
</Tabs>