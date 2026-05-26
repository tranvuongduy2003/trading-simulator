import { AlertCircleIcon, RotateCcwIcon } from 'lucide-react'

import { Alert, AlertDescription } from '@/components/ui/alert'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Spinner } from '@/components/ui/spinner'

import { resetPortfolioConsequences } from './reset-consequences'

type ResetPortfolioDialogProps = {
  open: boolean
  onOpenChange: (open: boolean) => void
  isPending: boolean
  errorMessage: string | null
  onConfirm: () => void
}

export function ResetPortfolioDialog({
  open,
  onOpenChange,
  isPending,
  errorMessage,
  onConfirm,
}: ResetPortfolioDialogProps) {
  const handleOpenChange = (nextOpen: boolean) => {
    if (!nextOpen && isPending) {
      return
    }

    if (!nextOpen) {
      onOpenChange(false)
      return
    }

    onOpenChange(nextOpen)
  }

  return (
    <AlertDialog open={open} onOpenChange={handleOpenChange}>
      <AlertDialogContent className="sm:max-w-md">
        <AlertDialogHeader className="text-left sm:text-left">
          <AlertDialogTitle>Reset portfolio?</AlertDialogTitle>
          <AlertDialogDescription className="text-left">
            This action cannot be undone. Confirming will:
          </AlertDialogDescription>
          <ul
            aria-label="Portfolio reset consequences"
            className="text-muted-foreground list-disc space-y-1.5 pl-5 text-sm"
          >
            {resetPortfolioConsequences.map((consequence) => (
              <li key={consequence}>{consequence}</li>
            ))}
          </ul>
        </AlertDialogHeader>

        {errorMessage ? (
          <Alert variant="destructive">
            <AlertCircleIcon />
            <AlertDescription>{errorMessage}</AlertDescription>
          </Alert>
        ) : null}

        <AlertDialogFooter>
          <AlertDialogCancel disabled={isPending}>Cancel</AlertDialogCancel>
          <AlertDialogAction
            variant="destructive"
            disabled={isPending}
            onClick={(event) => {
              event.preventDefault()
              onConfirm()
            }}
          >
            {isPending ? (
              <>
                <Spinner className="size-4" />
                Resetting…
              </>
            ) : (
              <>
                <RotateCcwIcon className="size-4" />
                Reset portfolio
              </>
            )}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
