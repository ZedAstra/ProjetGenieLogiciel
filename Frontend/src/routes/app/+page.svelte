<script lang="ts">
	import { goto } from "$app/navigation";
	import { getBearerToken, getChantier, getClient, getMe, logout, setChantier } from "$lib/App.svelte";
	import type { components } from "$lib/v1";
	import { onMount } from "svelte";
    import { Button, Overlay, ProgressCircle, SelectField } from "svelte-ux";

    let chantiers: components["schemas"]["CompactChantier"][] = $state([]);
    let options: {
        label: string;
        value: components["schemas"]["CompactChantier"];
    }[] = $state([]);
    let selected: components["schemas"]["CompactChantier"] | null = $state(null);
    $effect(() => {
        if(selected) {
            setChantier(selected.id!);
        }
    });

    onMount(async () => {
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
            return;
        }
        if (data) {
            chantiers = data;
            options = chantiers.map((project) => {
                return {
                    label: project.nom!,
                    value: project,
                };
            });
            if(options.length != 0) {
                selected = chantiers.find((project) => project.id == getChantier())!;
            }
        }
    });
    async function doLogout() {
        await logout();
        goto("/");
    }

    let me = getMe();
</script>

<div class="flex flex-col min-h-[calc(100vh-54px)]">
    <SelectField label="Chantier" bind:value={selected} options={options}
        placeholder="Sélectionner un chantier" clearable={false}/>
    <Button color="danger" variant="fill-outline" onclick={async () => doLogout()}>Déconnexion</Button>
    {#if me?.userRole == "Admin" || me?.userRole == "Chef"}
    <div class="flex flex-1"></div>
    <div class="flex flex-col justify-self-end">
        <h1>Zone de danger</h1>
        <div>
            <Button color="danger" variant="fill" onclick={async () => {
                const { response, error } = await getClient().DELETE("/app/projects/{id}", {
                    params: {
                        // @ts-ignore
                        header: {
                            Authorization: getBearerToken(),
                        },
                        path: {
                            id: selected!.id!,
                        },
                    },
                });
                if (error) {
                    console.error(error);
                    alert("Erreur lors de la suppression du chantier");
                    return;
                }
                if (response.status == 204) {
                    goto("/");
                }
            }}>Supprimer le chantier</Button>
        </div>
    </div>
    {/if}
</div>