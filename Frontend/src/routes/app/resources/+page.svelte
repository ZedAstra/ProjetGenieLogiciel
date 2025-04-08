<script lang="ts">
	import { Button } from "$lib/components/ui/button";
    import { appFetch, getUserRole } from "$lib/app.svelte";
	import { Input } from "$lib/components/ui/input";

    let resources: {
        nom: string,
        quantité: number,
        unité: string
    }[] = $state([]);
    let newResource = $state({
        nom: "",
        quantité: 0,
        unité: ""
    });

    async function getResources() {
        var response = await appFetch("/management/resources", {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
        })
        if (response.ok) {
            resources = await response.json();
        } else {
            console.error("Failed to fetch resources");
        }
    }

    async function deleteResource(resource: typeof resources[0]) {
        var response = await appFetch("/management/resources/" + resource.nom, {
            method: "DELETE",
        })
        if (response.ok) {
            resources = resources.filter(r => r.nom !== resource.nom);
        } else {
            console.error("Failed to delete resource");
        }
    }

    async function saveResource(resource: typeof resources[0]) {
        const input : HTMLInputElement = document.getElementById("editBox-" + resource.nom) as HTMLInputElement;
        let modified = {
            nom: resource.nom,
            quantité: resource.quantité,
            unité: resource.unité
        };
        if (input) {
            modified.quantité = parseFloat(input.value);
        }
        else {
            console.error("Input not found");
            return;
        }
        let json = JSON.stringify(modified);
        var response = await appFetch("/management/resources/update", {
            method: "POST",
            body: json,
            headers: {
                "Content-Type": "application/json",
            },
        })
        if (response.ok) {
            console.log("Resource updated successfully");
            for (let i = 0; i < resources.length; i++) {
                if (resources[i].nom === resource.nom) {
                    resources[i].quantité = modified.quantité;
                    break;
                }
            }
        } else {
            console.error("Failed to update resource");
        }
    }

    async function addResource(resource: typeof resources[0]) {
        let formData = new FormData();
        formData.append("name", newResource.nom);
        formData.append("quantity", newResource.quantité.toString());
        formData.append("unit", newResource.unité);
        var response = await appFetch("/management/resources/add", {
            method: "POST",
            body: formData,
        })
        if (response.ok) {
            resources.push({
                nom: newResource.nom,
                quantité: newResource.quantité,
                unité: newResource.unité
            });
        } else {
            console.error("Failed to add resource");
        }
        newResource.nom = "";
        newResource.quantité = 0;
        newResource.unité = "";
        
    }

    getResources();

    function anyOf(value: string, ...args: string[]) {
        return args.includes(value);
    }

</script>

<div class="flex flex-col">
    <div class="prose">
        <h1>Ressources</h1>
    </div>
    {#if resources.length !== 0}
        {#each resources as resource}
            <div class="flex flex-row justify-between items-center p-4 border-b">
                <div class="flex-1 flex flex-row justify-between">
                    {#if anyOf(getUserRole(), "Admin", "Chef", "Partenaire", "Artisan")}
                    <div>
                        <p class="font-bold">{resource.nom}</p>
                        <span class="inline-flex items-center gap-2">
                            <Input id="editBox-{resource.nom}" class="h-8" value={resource.quantité}></Input>
                            <p>{resource.unité}</p>
                        </span>
                    </div>
                    
                    <div class="flex flex-row">
                        <Button variant="outline" class="" onclick={() => saveResource(resource)}>
                            Enregistrer
                        </Button>
                        <Button variant="destructive" class="" onclick={() => deleteResource(resource)}>
                            Supprimer
                        </Button>
                    </div>
                    {:else}
                    <div>
                        <p class="font-bold">{resource.nom}</p>
                        <span class="inline-flex items-center gap-1">
                            <p>{resource.quantité}</p>
                            <p>{resource.unité}</p>
                        </span>
                    </div>
                    {/if}
                </div>
            </div>
        {/each}
    {:else}
        <div class="flex flex-row justify-center items-center p-4 border-b">
            <p>Aucune ressource disponible</p>
        </div>
    {/if}
    {#if anyOf(getUserRole(), "Admin", "Chef", "Partenaire", "Artisan")}
        <div class="flex flex-row justify-between items-center p-4 border-b">
            <Input placeholder="Nom de la ressource" bind:value={newResource.nom}></Input>
            <Input placeholder="Quantité" type="number" bind:value={newResource.quantité}></Input>
            <Input placeholder="Unité" bind:value={newResource.unité}></Input>
            <Button class="ml-4" onclick={() => addResource(newResource)}>
                Ajouter
            </Button>
        </div>
    {/if}
</div>