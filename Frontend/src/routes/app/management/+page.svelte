<script lang="ts">
	import { formBodySerializer, getBearerToken, getChantier, getClient, getMe, isAnyOf } from "$lib/App.svelte";
	import type { components } from "$lib/v1";
	import { onMount } from "svelte";
	import { Button, Checkbox, Dialog, MultiSelect, Table, Tabs, type MenuOption } from "svelte-ux";
	import Resources from "./Resources.svelte";

    let everyone: components["schemas"]["SafeUtilisateur"][] = $state([]);
    let membres: components["schemas"]["SafeUtilisateur"][] = $state([]);
    let tableData: {
        ID: number;
        Nom: string;
        Role: string;
    }[] = $state([]);
    let me = getMe();

    onMount(refresh);

    async function refresh() {
        const { data, error } = await getClient().GET("/app/projects/{id}/members", {
            params: {
                // @ts-ignore
                header: {
                    Authorization: getBearerToken(),
                },
                path: {
                    id: getChantier(),
                },
            },
        })
        if (error) {
            console.error(error);
            return;
        }
        if (data) {
            membres = data;
            tableData = membres.map((membre) => {
                return {
                    ID: membre.id!,
                    Nom: `${membre.firstName} ${membre.lastName}`,
                    Role: membre.userRole!,
                };
            });
        }
        const res = await getClient().GET("/app/everyone", {
            params: {
                // @ts-ignore
                header: {
                    Authorization: getBearerToken(),
                },
            },
        })
        if (res.error) {
            console.error(res.error);
            return;
        }
        if (res.data) {
            everyone = res.data.filter((user) => {
                return !membres.some((membre) => membre.id == user.id);
            });
        }
    }

    let options: {
        label: string;
        value: string;
    }[] = [
        {
            label: "Membres",
            value: "members",
        },
        {
            label: "Ressources",
            value: "resources",
        },
    ];
    let currentTab: string = $state("members");

    let addMemberDialogOpen = $state(false);
    let selectedMembers: number[] = $state([]);
    let selectedMembersForRemoval: number[] = $state([]);

    $effect(() => {
        if (selectedMembersForRemoval.length > 0) {
            console.log("Selected members for removal: ", selectedMembersForRemoval);
        }
    });

    async function addMembers() {
        if(selectedMembers.length == 0) {
            alert("Veuillez sélectionner au moins un membre");
            return;
        }
        const { data, error } = await getClient().POST("/app/projects/{id}/add_members", {
            params: {
                // @ts-ignore
                header: {
                    Authorization: getBearerToken(),
                },
                path: {
                    id: getChantier(),
                },
            },
            body: selectedMembers
        });
        if (error) {
            console.error(error);
            return;
        }
        if (data) {
            alert("Membres ajoutés avec succès");
            addMemberDialogOpen = false;
            selectedMembers = [];
            refresh();
        }
    }
    async function removeMembers() {
        const { data, error } = await getClient().POST("/app/projects/{id}/remove_members", {
            params: {
                // @ts-ignore
                header: {
                    Authorization: getBearerToken(),
                },
                path: {
                    id: getChantier(),
                },
            },
            body: selectedMembersForRemoval
        });
        if (error) {
            console.error(error);
            return;
        }
        if (data) {
            alert("Membres retirés avec succès");
            selectedMembersForRemoval = [];
            refresh();
        }
    }
</script>

<div>
    <Tabs {options} bind:value={currentTab}>
        <svelte:fragment slot="content" let:value>
            {#if currentTab == "members" && me?.userRole == "Admin" || me?.userRole == "Chef"}
                <Button class="my-2 ml-2" variant="fill-light" color="primary" onclick={() => addMemberDialogOpen = true}>Ajouter des membres</Button>
                {#if selectedMembersForRemoval.length > 0}
                <Button class="my-2 ml-2" variant="fill-light" color="danger" onclick={removeMembers}>Retirer les membres sélectionnés</Button>
                {/if}
            <Dialog bind:open={addMemberDialogOpen} classes={{dialog: "p-2"}}>
                <div class="flex flex-col w-96 gap-2 mb-2">
                    {#each everyone as user}
                        <Checkbox
                            value={user.id}
                            bind:group={selectedMembers}>{`${user.firstName} ${user.lastName} - ${user.userRole}`}</Checkbox>
                    {/each}
                </div>
                <Button variant="fill-light" color="primary" onclick={addMembers}>Confirmer</Button>
            </Dialog>

            <Table data={tableData}
            columns={[
                { name: "ID" },
                { name: "Nom" },
                { name: "Role" },
                { name: "" },
            ]}>
                <tbody slot="data" let:columns let:data let:getCellValue>
                    {#each data ?? [] as rowData, rowIndex}
                    <tr class="tabular-nums">
                        {#each columns as column (column.name)}
                            {@const value = getCellValue(column, rowData, rowIndex)}
                            {#if column.name == ""}
                            <td class="flex max-w-fit">
                                {#if !isAnyOf(rowData.Role, "Admin", "Chef")}
                                <Checkbox
                                    value={rowData.ID}
                                    bind:group={selectedMembersForRemoval}
                                    class="w-4 h-4"
                                    color="primary"
                                    variant="fill-light" />
                                {/if}
                            </td>
                            {:else}
                            <td>{value}</td>
                            {/if}
                        {/each}
                    </tr>
                    {/each}
                </tbody>
            </Table>
            {:else if currentTab == "resources"}
            <Resources />
            {/if}
        </svelte:fragment>
        
    </Tabs>
</div>