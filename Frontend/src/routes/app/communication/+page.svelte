<script lang="ts">
	import { appFetch, getUserId, getUserRole } from "$lib/app.svelte";
    import { LoaderCircle, RotateCcw } from "lucide-svelte";
	import { Button } from "$lib/components/ui/button";
    import * as Tabs from "$lib/components/ui/tabs";
    import * as Dialog from "$lib/components/ui/dialog";
    import * as Card from "$lib/components/ui/card";
    import { Label } from "$lib/components/ui/label";
    import { Input } from "$lib/components/ui/input";
    import ShadEditor from '$lib/components/shad-editor/shad-editor.svelte';
    import { getLocalTimeZone, today } from "@internationalized/date";
    import { Calendar } from "$lib/components/ui/calendar/index.js";
 
    let todayDate = today(getLocalTimeZone());
	import { writable } from "svelte/store";
	import { onMount } from "svelte";

    let tab: "news" | "events" | "complaints" | "reports" = $state("news");
    let userRole = getUserRole();

    let isLoading: boolean = $state(false);

    let newNewsOpen: boolean = $state(false);
    let newNewsTitle: string = $state("");
    let newNewsContent = $state("");
    async function publishNews() {
        isLoading = true;
        var data = {
            title: newNewsTitle,
            content: newNewsContent,
        }
        var result = await appFetch("/communication/news/create", {
            headers: {
                "Content-Type": "application/json",
            },
            method: "POST",
            mode: "cors",
            body: JSON.stringify(data),
        })
        isLoading = false;
        if(result.status === 201) {
            newNewsTitle = "";
            newNewsContent = "";
            newNewsOpen = false;
            refresh();
        }
        else {
            if(result.status == 401) {
                alert("Vous n'avez pas le droit de publier une annonce!");
            }
            else alert("Erreur lors de la publication de l'annonce");
        }
        
    }
    async function deleteNews(id: string) {
        isLoading = true;
        var result = await appFetch("/communication/news/" + id, {
            method: "DELETE",
        })
        isLoading = false;
        if(result.status === 202) {
            refresh();
        }
        else {
            alert("Erreur lors de la suppression de l'annonce");
        }
    }

    let newComplaint = $state({
        title: "",
        author: "",
        content: "",
    });
    let newComplaintOpen: boolean = $state(false);
    async function publishComplaint() {
        isLoading = true;
        newComplaint.author = getUserId();
        var result = await appFetch("/communication/complaints/create", {
            headers: {
                "Content-Type": "application/json",
            },
            method: "POST",
            mode: "cors",
            body: JSON.stringify(newComplaint),
        })
        isLoading = false;
        if(result.status === 201) {
            newComplaint = {
                title: "",
                author: "",
                content: "",
            };
            newComplaintOpen = false;
            refresh();
        }
        else {
            alert("Erreur lors de la publication de la plainte");
        }
        
    }

    let newsViewOpen: boolean = $state(false);
    let news: {
        id: string,
        titre: string,
        publication: Date,
        contenu: string,
    }[] = [];
    let currentNews: {
        id: string,
        titre: string,
        publication: Date,
        contenu: string,
    } | null = $state(null);

    let complaintsViewOpen: boolean = $state(false);
    let complaints: {
        id: string,
        titre: string,
        autheur: string,
        contenu: string,
    }[] = [];
    let currentComplaint: {
        id: string,
        titre: string,
        autheur: string,
        contenu: string,
    } | null = $state(null);

    async function getNews(): Promise<{
        id: string,
        titre: string,
        publication: Date,
        contenu: string,
    }[]> {
        var result = await appFetch("/communication/news", {
            method: "GET",
        })
        if(result.ok) {
            return await result.json();
        }
        else return [];
    }
    async function getComplaints(): Promise<{
        id: string,
        titre: string,
        autheur: string,
        contenu: string,
    }[]> {
        var result = await appFetch("/communication/complaints", {
            method: "GET",
        })
        if(result.ok) {
            return await result.json();
        }
        else return [];
    }

    async function refresh() {
        if(isLoading) return;
        
        isLoading = true;
        news = await getNews();
        for(let _new of news) {
            _new.publication = new Date(_new.publication)
        }
        news.sort((a, b) => b.publication.getTime() - a.publication.getTime());
        isLoading = true;
        complaints = await getComplaints();
        isLoading = false;
    }

    onMount(() => {
        refresh();
    });
</script>

<div class="prose max-w-full h-full flex flex-col max-h-screen">
    <div class="flex flex-row justify-between">
        <h1>Communication</h1>
        <Button onclick={refresh}>
            Rafraichir
            <RotateCcw />
        </Button>
    </div>  

    <Tabs.Root bind:value={tab} class="flex-1 flex flex-col">
        <Tabs.List class="w-full flex flex-row justify-stretch">
            <Tabs.Trigger class="flex-1" value="news">Annonces</Tabs.Trigger>
            <!-- <Tabs.Trigger class="flex-1" value="events">Evenements</Tabs.Trigger> -->
            <Tabs.Trigger class="flex-1" value="complaints">Plaintes</Tabs.Trigger>
            <Tabs.Trigger class="flex-1" value="reports">Rapports</Tabs.Trigger>
        </Tabs.List>

        <Tabs.Content value="news" class="flex-1">
            {#if userRole === "Admin" || userRole === "Chef" || userRole === "Partenaire"}
                
                <Dialog.Root bind:open={newNewsOpen}>
                    <Dialog.Trigger>
                        <Button class="mb-4">Créer une annonce +</Button>
                    </Dialog.Trigger>
                    <Dialog.Content class="w-screen min-w-full">
                        <Dialog.Header>
                            <Dialog.Title>Publier une annonce</Dialog.Title>
                          </Dialog.Header>

                        <div class="grid w-full items-center gap-4">
                            <div class="flex flex-col space-y-1.5">
                                <Label for="titre">Titre</Label>
                                <Input id="titre" placeholder="Titre de l'annonce" bind:value={newNewsTitle}/>
                            </div>
                            <div class="flex flex-col space-y-1.5">
                                <ShadEditor class="h-[40rem]" bind:content={newNewsContent} />
                            </div>
                        </div>

                        <Dialog.Footer>
                            <Button type="submit" onclick={publishNews}>Envoyer</Button>
                        </Dialog.Footer>
                    </Dialog.Content>
                </Dialog.Root>
            {/if}
            {#if isLoading}
                <div class="flex w-full h-full justify-center items-center">
                    <LoaderCircle class="animate-spin" size={32}/>
                </div>
            {:else}
                {#if news.length === 0}
                    <p>Aucune annonce...</p>
                {:else}
                    <div class="flex flex-col gap-4 {(userRole === "Admin" || userRole === "Chef" || userRole === "Partenaire" ? 
                    "max-h-[calc(100vh-208px)]" : 
                    "max-h-[calc(100vh-160px)]")} overflow-y-scroll">
                        <Dialog.Root bind:open={newsViewOpen}>
                            <Dialog.Content class="w-[calc(100vw-32px)] min-w-[calc(100vw-32px)]  p-4">
                                <Dialog.Header>
                                    <Dialog.Title>{currentNews?.titre}</Dialog.Title>
                                    <p>{currentNews?.publication.toLocaleString()}</p>
                                </Dialog.Header>
                        
                                {@html currentNews?.contenu}
                            </Dialog.Content>
                        </Dialog.Root>
                        {#each news as annonce}
                        <Card.Root class="cursor-pointer" onclick={() => {
                            currentNews = annonce;
                            newsViewOpen = true;
                        }}>
                            <Card.Header>
                                <Card.Title>{annonce.titre}</Card.Title>
                            </Card.Header>
                            <Card.Content>

                            </Card.Content>
                            <Card.Footer>
                                {annonce.publication.toLocaleString()}
                            </Card.Footer>
                        </Card.Root>
                        {/each}
                    </div>
                {/if}
            {/if}
        </Tabs.Content>

        <Tabs.Content value="complaints">
            <Dialog.Root bind:open={newComplaintOpen}>
                <Dialog.Trigger>
                    <Button class="mb-4">Créer une plainte +</Button>
                </Dialog.Trigger>
                <Dialog.Content class="w-screen min-w-full">
                    <Dialog.Header>
                        <Dialog.Title>Envoyer une plainte</Dialog.Title>
                        </Dialog.Header>

                    <div class="grid w-full items-center gap-4">
                        <div class="flex flex-col space-y-1.5">
                            <Label for="titre">Titre</Label>
                            <Input id="titre" placeholder="Titre de la plainte" bind:value={newComplaint.title}/>
                        </div>
                        <div class="flex flex-col space-y-1.5">
                            <ShadEditor class="h-[40rem]" bind:content={newComplaint.content} />
                        </div>
                    </div>

                    <Dialog.Footer>
                        <Button type="submit" onclick={publishComplaint}>Envoyer</Button>
                    </Dialog.Footer>
                </Dialog.Content>
            </Dialog.Root>
            {#if isLoading}
                <div class="flex w-full h-full justify-center items-center">
                    <LoaderCircle class="animate-spin" size={32}/>
                </div>
            {:else}
                {#if complaints.length === 0}
                    <p>Aucune plaintes...</p>
                {:else}
                    <div class="flex flex-col gap-4 max-h-[calc(100vh-208px)] overflow-y-scroll">
                        <Dialog.Root bind:open={complaintsViewOpen}>
                            <Dialog.Content class="w-[calc(100vw-32px)] min-w-[calc(100vw-32px)]  p-4">
                                <Dialog.Header>
                                    <Dialog.Title>{currentComplaint?.titre}</Dialog.Title>
                                </Dialog.Header>
                        
                                {@html currentComplaint?.contenu}
                            </Dialog.Content>
                        </Dialog.Root>
                        {#each complaints as complaint}
                        <Card.Root class="cursor-pointer" onclick={() => {
                            currentComplaint = complaint;
                            complaintsViewOpen = true;
                        }}>
                            <Card.Header>
                                <Card.Title>{complaint.titre}</Card.Title>
                            </Card.Header>
                            <Card.Content>

                            </Card.Content>
                        </Card.Root>
                        {/each}
                    </div>
                {/if}
            {/if}
        </Tabs.Content>
        <Tabs.Content value="reports">Rapports</Tabs.Content>
    </Tabs.Root>
</div>