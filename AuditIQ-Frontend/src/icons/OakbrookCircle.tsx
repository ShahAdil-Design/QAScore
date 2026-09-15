import type { SVGProps } from 'react'

const OakbrookCircle = (props: SVGProps<SVGSVGElement>) => (
  <svg
    viewBox="0 0 344 344"
    width={344}
    height={344}
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
    {...props}
  >
    <path
      d="M88.478 322.353a35.345 35.345 0 0 0 13.125 4.221 35.324 35.324 0 0 0 39.142-31.057 35.311 35.311 0 0 0-17.95-34.913 101.347 101.347 0 0 0 137.774-39.366 101.303 101.303 0 0 0 8.863-77.025 101.318 101.318 0 0 0-48.198-60.738 35.32 35.32 0 0 1 7.456-64.837 35.332 35.332 0 0 1 26.861 3.088 171.965 171.965 0 0 1 81.836 103.086 171.94 171.94 0 0 1-15.037 130.751 171.975 171.975 0 0 1-103.101 81.824 172.006 172.006 0 0 1-130.77-15.034Z"
      fill="url(#oakbrook-circle-gradient1)"
    />
    <path
      d="M255.522 21.647a35.328 35.328 0 0 0-52.266 26.836 35.313 35.313 0 0 0 17.95 34.913 101.349 101.349 0 0 0-77.04-8.841 101.33 101.33 0 0 0-60.734 48.207 101.303 101.303 0 0 0-8.863 77.025 101.321 101.321 0 0 0 48.198 60.738 35.32 35.32 0 0 1 13.721 48.03 35.327 35.327 0 0 1-48.038 13.719A171.965 171.965 0 0 1 6.614 219.188 171.937 171.937 0 0 1 21.651 88.437 171.981 171.981 0 0 1 124.752 6.613a172.007 172.007 0 0 1 130.77 15.034Z"
      fill="url(#oakbrook-circle-gradient2)"
    />
    <defs>
      <linearGradient
        id="oakbrook-circle-gradient1"
        x1={273.437}
        y1={60.273}
        x2={94.82}
        y2={367.522}
        gradientUnits="userSpaceOnUse"
      >
        <stop stopColor="#E0DFFF" />
        <stop offset={0.461} stopColor="#514FFF" />
        <stop offset={0.601} stopColor="#514FFF" />
        <stop offset={1} stopColor="#514FFF" stopOpacity={0.36} />
      </linearGradient>
      <linearGradient
        id="oakbrook-circle-gradient2"
        x1={70.564}
        y1={283.727}
        x2={249.181}
        y2={-23.522}
        gradientUnits="userSpaceOnUse"
      >
        <stop stopColor="#E0DFFF" />
        <stop offset={0.461} stopColor="#514FFF" />
        <stop offset={0.601} stopColor="#514FFF" />
        <stop offset={1} stopColor="#514FFF" stopOpacity={0.36} />
      </linearGradient>
    </defs>
  </svg>
)

export default OakbrookCircle
