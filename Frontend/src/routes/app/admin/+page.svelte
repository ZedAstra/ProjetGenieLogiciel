<script lang="ts">
	import { getBearerToken, getChantier, getClient } from "$lib/App.svelte";
	import { onMount } from "svelte";
    import { Button, Dialog, SelectField, Table, Tabs, TextField } from "svelte-ux";
	import { get } from "svelte/store";

    let options: {
        label: string;
        value: string;
    }[] = [
        {
            label: "Utilisateurs",
            value: "users",
        },
        {
            label: "Scalar",
            value: "scalar",
        },
        {
            label: "Outils",
            value: "tools",
        }
    ];
    let currentTab: string = $state("users");
    let scalar: HTMLIFrameElement | null = $state(null);

    let tableData: {
        Id: number;
        Nom: string;
        Prenom: string;
        Email: string;
        MotDePasse: string;
        Role: string;
        Actions: string;
    }[] = $state([]);

    onMount(loadUsers);

    async function loadUsers() {
        const { data, error } = await getClient().GET("/admin/utilisateur", {
            params: {
                // @ts-ignore
                header: {
                    Authorization: getBearerToken(),
                }
            },
        })
        if (error) {
            console.error("Error fetching data: ", error);
            return;
        }
        if (data) {
            tableData = data.map((user) => ({
                Id: user.id!,
                Nom: user.nom,
                Prenom: user.prenom,
                Email: user.email,
                MotDePasse: user.motDePasse,
                Role: user.roleUtilisateur,
                Actions: ""
            }));
        }
    }

    let selectedUser: {
        Id: number;
        Nom: string;
        Prenom: string;
        Email: string;
        MotDePasse: string;
        Role: "None" | "Admin" | "Chef" | "Partenaire" | "Ouvrier";
    } = $state({
        Id: -1,
        Nom: "",
        Prenom: "",
        Email: "",
        MotDePasse: "",
        Role: "None",
    });
    let userDialogOpen = $state(false);
    let dialogType: "create" | "update" | "delete" = $state("create");

    let roles = [
        {
            label: "Aucun",
            value: "None"
        },
        {
            label: "Admin",
            value: "Admin",
        },
        {
            label: "Chef",
            value: "Chef",
        },
        {
            label: "Partenaire",
            value: "Partenaire",
        },
        {
            label: "Ouvrier",
            value: "Ouvrier",
        },
    ]

    let isLoading = $state(false);
    async function confirmChoice() {
        if(dialogType == "create" || dialogType == "update")
        {
            if(!selectedUser.Nom || !selectedUser.Prenom || !selectedUser.Email || !selectedUser.MotDePasse || !selectedUser.Role)
            {
                alert("Veuillez remplir tous les champs");
                return;
            }
            if(dialogType == "create")
            {
                isLoading = true;
                const { data, error } = await getClient().POST("/admin/utilisateur/create", {
                    params: {
                        // @ts-ignore
                        header: {
                            Authorization: getBearerToken(),
                        }
                    },
                    body: {
                        nom: selectedUser.Nom,
                        prenom: selectedUser.Prenom,
                        email: selectedUser.Email,
                        motDePasse: selectedUser.MotDePasse,
                        roleUtilisateur: selectedUser.Role,
                    }
                })
                if (error) {
                    console.error("Error creating user: ", error);
                    alert("Erreur lors de la création de l'utilisateur: " + error);
                    return;
                }
                if(data)
                {
                    await loadUsers();
                    isLoading = false;
                    userDialogOpen = false;
                }
            }
            else if(dialogType == "update")
            {
                const { data, error } = await getClient().PUT("/admin/utilisateur/{id}", {
                    params: {
                        // @ts-ignore
                        header: {
                            Authorization: getBearerToken(),
                        },
                        path: {
                            id: selectedUser.Id,
                        }
                    },
                    body: {
                        id: selectedUser.Id,
                        nom: selectedUser.Nom,
                        prenom: selectedUser.Prenom,
                        email: selectedUser.Email,
                        motDePasse: selectedUser.MotDePasse,
                        roleUtilisateur: selectedUser.Role,
                    }
                })
                if (error) {
                    console.error("Error updating user: ", error);
                    alert("Erreur lors de la mise à jour de l'utilisateur: " + error);
                    return;
                }
                if(data)
                {
                    await loadUsers();
                    isLoading = false;
                    userDialogOpen = false;
                }
            }
        }
        else if(dialogType == "delete") {
            const { response } = await getClient().DELETE("/admin/utilisateur/{id}", {
                params: {
                    // @ts-ignore
                    header: {
                        Authorization: getBearerToken(),
                    },
                    path: {
                        id: selectedUser.Id,
                    }
                }
            });
            console.log("Deleting user: ", response);
            if(response.status == 200)
            {
                await loadUsers();
                userDialogOpen = false;
            }
            else
            {
                alert("Erreur lors de la suppression de l'utilisateur: " + response.statusText);
            }
        }
    }
</script>

<!-- svelte-ignore attribute_quoted -->
<Tabs {options} bind:value={currentTab} classes={{ content: "h-[calc(100vh-84px)]"}}>
    <svelte:fragment slot="content" let:value >
        {#if currentTab == "users"}
        <Dialog bind:open={userDialogOpen} persistent>
            <div slot="title">
               {dialogType == "create" ? "Créer un utilisateur" : dialogType == "update" ? "Modifier un utilisateur" : "Supprimer un utilisateur"} 
            </div>
            {#if dialogType == "delete"}
            <div>Êtes-vous sûr de vouloir supprimer l'utilisateur {selectedUser.Prenom + " " + selectedUser.Nom} ?</div>
            {:else}
            <TextField label="Prénom" bind:value={selectedUser.Prenom} />
            <TextField label="Nom" bind:value={selectedUser.Nom} />
            <TextField label="Email" bind:value={selectedUser.Email} />
            <TextField label="Mot de passe" bind:value={selectedUser.MotDePasse} type="password" />
            <SelectField label="Rôle" options={roles} bind:value={selectedUser.Role} />
            {/if}
            <div slot="actions">
                <Button variant="fill" color="secondary" onclick={() => {
                    userDialogOpen = false;
                }}>Annuler</Button>
                <Button variant="fill" color="{(dialogType == "delete" ? "danger" : "primary")}" onclick={confirmChoice}>Confirmer</Button>
            </div>
        </Dialog>
        <Button class="my-2 ml-2" variant="fill-light" color="primary" onclick={() => {
            userDialogOpen = true;
            selectedUser = {
                Id: -1,
                Nom: "",
                Prenom: "",
                Email: "",
                MotDePasse: "",
                Role: "None",
            };
            dialogType = "create";
        }}>Créer un utilisateur</Button>
        <Table data={tableData}
            columns={[
                { name: "Id" },
                { name: "Nom" },
                { name: "Prenom" },
                { name: "Email" },
                { name: "MotDePasse" },
                { name: "Role" },
                { name: "Actions" }
            ]}>
            <tbody slot="data" let:columns let:data let:getCellValue>
                {#each data ?? [] as rowData, rowIndex}
                <tr class="tabular-nums">
                    {#each columns as column (column.name)}
                        {@const value = getCellValue(column, rowData, rowIndex)}
                        {#if column.name == "Actions"}
                        <td class="flex max-w-fit">
                            <Button variant="fill" color="secondary" onclick={() => {
                                selectedUser = rowData as any;
                                dialogType = "update";
                                userDialogOpen = true;
                            }}>Modifier</Button>
                            <Button variant="fill" color="danger"onclick={() => {
                                selectedUser = rowData as any;
                                dialogType = "delete";
                                userDialogOpen = true;
                            }}>Suprimmer</Button>
                        </td>
                        {:else}
                        <td>{value}</td>
                        {/if}
                    {/each}
                </tr>
                {/each}
            </tbody>
        </Table>
        {:else if currentTab == "scalar"}
        <iframe title="scalar" id="scalar" src="https://localhost:7149/scalar" bind:this={scalar} width="100%" height="100%"></iframe>
        {/if}
        {#if currentTab == "tools"}
        <div class="size-full p-2">
            <Button variant="fill-light" color="secondary" onclick={() => {
                // Copy the bearer token to the clipboard
                const bearerToken = getBearerToken().split(" ")[1];
                if(!bearerToken) {
                    console.error("Bearer token not found");
                    return;
                }
                navigator.clipboard.writeText(bearerToken).then(() => {
                    alert("Token copié dans le presse-papier");
                }).catch(err => {
                    console.error("Failed to copy: ", err);
                });
            }}>Copier le token Jwt 🗒️</Button>
        </div>
        {/if}
    </svelte:fragment>
</Tabs>

<style>
    :global(.column-Actions) {
        width: 200px;
    }
</style>